# convert-to-pdf
converts any document into pdf so that it can render on the browser

inspired from: https://github.com/shelfio/libreoffice-lambda-layer

This solution contains:-
#1. A Lambda app that take S3 object key and bucket as an inputs and convert the object to pdf file, saves in the same location along with the given key.
#2. C# client that uploads a file to S3 and invoked lambda in order to convert the uploaded file into Pdf

Lambda App 

    Runtime - Node.js
    
    Dependecies :
	#1. @shelf/aws-lambda-libreoffice - NPM package
	#2. libreoffice - lambda layer 
	
    Settings:
	#1. Lambda layer should be added
	#2. S3 read/write permission 
	#3. timeout - 5 mins
	#4. memory min 1536 MB
