using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace PdfConvertWebApp.Controllers
{
    public class HomeController : Controller
    {
        private static string _bucket;
        private static AmazonS3Client _s3Client;
        private static AmazonLambdaClient _lambdaClient;

        public HomeController(IConfiguration configuration)
        {
            if (_s3Client != null && _lambdaClient != null) return;
            var aws = configuration.GetSection("AWS");
            var awsCredentials = new BasicAWSCredentials(aws["AccessKey"], aws["Secret"]);
            var region = RegionEndpoint.GetBySystemName(aws["Region"]);
            _s3Client = new AmazonS3Client(awsCredentials, region);
            _lambdaClient = new AmazonLambdaClient(awsCredentials, region);
            _bucket = aws["Bucket"];
        }

        [HttpPost]
        public async Task<IActionResult> CovertDoc(List<IFormFile> files)
        {
            if (!files.Any()) return View();
            var doc = files[0];
            var key = doc.FileName;//key is full folder path on S3 

            var utility = new TransferUtility(_s3Client);
            await utility.UploadAsync(doc.OpenReadStream(), _bucket, key);

            var req = new InvokeRequest
            {
                FunctionName = "your_lambda_funtion_name",
                Payload = JsonSerializer.Serialize(new { key, bucket = _bucket }),
            };
            var res = await _lambdaClient.InvokeAsync(req);
            using var sr = new StreamReader(res.Payload);
            var resString = await sr.ReadToEndAsync();
            var pdfKey = JsonSerializer.Deserialize<string>(resString);// my lambda return converted pdf S3 key
            
            var preSignedUrlRequest = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = pdfKey,
                Expires = DateTime.Now.AddDays(1),
                ResponseHeaderOverrides = new ResponseHeaderOverrides()
                {
                    ContentType = "application/force-download",
                    ContentDisposition = "attachment;filename=" + "download_file_name"
                }
            };
            var preSignedUrl = _s3Client.GetPreSignedURL(preSignedUrlRequest);//downloadable S3 file url 
            
            ViewBag.pdfUrl = preSignedUrl;
            return View("Convert-Pdf");
        }
    }
}
