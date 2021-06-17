# Cloud OCR SDK 

ABBYY Cloud OCR SDK provides Web API that can be easily used in C#, Java, Python, or any other development tool supporting communication over network. 

This repo contains a set of samples in different programming languages showing how to create a simple client application using API V1 for processing image with the specified parameters and exporting the result.

Find the [API V1 reference](https://www.ocrsdk.com/documentation/api-reference/process-image-method/) in the ABBYY Cloud OCR SDK [documentation](https://www.ocrsdk.com/documentation/).

For more information about the product visit [ABBYY Cloud OCR SDK website](https://www.ocrsdk.com/). 

## Features

- Text recognition
  - full-page and zonal OCR (printed text) for 200+ languages
  - ICR (hand-printed text)
- Document conversion
  - convert image/PDF to searchable PDF, PDF/A and Microsoft Word, Excel, PowerPoint
- Data extraction
  - Barcode recognition 
  - Business card recognition
  - ID recognition (MRZ)

## Quick start guide

To observe and use the samples fo image recognition, do the following:

1. [Register](https://cloud.ocrsdk.com/Account/Register) on the ABBYY Cloud OCR SDK website and create your Application. You will need the Application ID and Application Password to run any of the code samples.
2. Download the samples from this repo.
3. Observe the API V1 and implement your application using ABBYY OCR and capturing technologies.

## Web API versions 

Currently ABBYY Cloud OCR SDK provides 2 Web API versions:
* V1 (XML response format)
* V2 (JSON response format)

Find the [full V1 and V2 difference list](https://www.ocrsdk.com/documentation/faq/#v1v2diff) in the documentation.

This repo contains samples, supporting v1 version only.

Investigate the [cloudsdk-client-dotnet](https://github.com/abbyysdk/cloudsdk-client-dotnet) repo for the client library and sample using the Web API (v2). 


## Supported export formats

You can export recognized text to the following formats:
- TXT
- RTF
- DOCX
- XLSX
- PPTX
- PDF
- PDF/A-1b
- XML
- ALTO
- vCard
- CSV


## Supported text types

ABBYY Cloud OCR SDK detects on the image the following types of text:
- normal
- typewriter
- matrix
- index
- handprinted
- ocrA
- ocrB
- e13b
- cmc7
- gothic

## License
Copyright © 2019 ABBYY Production LLC

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
