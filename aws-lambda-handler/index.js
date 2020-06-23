const { convertTo, canBeConvertedToPDF } = require('@shelf/aws-lambda-libreoffice');
const fs = require('fs');
const { parse } = require('path');
const { writeFileSync, readFileSync } = require('fs');
const {S3} = require('aws-sdk');

exports.handler = async ({ key, bucket }) => {
  // assuming there is a document.docx file inside /tmp dir
  // original file will be deleted afterwards

  try {
    if (!fs.existsSync('/tmp')) {
      fs.mkdirSync('/tmp')
    }

  } catch (err) {
    console.error(err)
  }
 // bucket = "my-bucket-test";
  //key  = "tes-file.txt";
  const s3 = new S3({ params: { Bucket: bucket } });


  let path = key.substring(0, key.lastIndexOf('/') + 1);
  let filename = key.substring(key.lastIndexOf('/') + 1);
  const { Body: inputFileBuffer } = await s3.getObject({ Key: key }).promise();
  writeFileSync(`/tmp/${filename}`, inputFileBuffer);

  if (!canBeConvertedToPDF(filename)) {
    return false;
  }
  return convertTo(filename, 'pdf').then(async _pdf => {
    const outputFilename = `${parse(filename).name}.pdf`;
    const outputFileBuffer = readFileSync(`/tmp/${outputFilename}`);


    await s3.upload({
      Key: `${path}${outputFilename}`,
      Body: outputFileBuffer,
      ContentType: 'application/pdf'
    })
      .promise();
    return `${path}${outputFilename}`
  }); //pdf
};
