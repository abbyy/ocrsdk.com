using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;

using NDesk.Options;
using Abbyy.CloudOcrSdk;

namespace ConsoleTest
{
    class Program
    {
        private static void displayHelp()
        {
            Console.WriteLine(
@"Usage:
ConsoleTest.exe [common options] <source_dir|file> <target>
  Perform full-text recognition of document

ConsoleTest.exe --asDocument [common options] <source_dir|file> <target_dir>
  Recognize file or directory treating each subdirectory as a single document

ConsoleTest.exe --asTextField [common options] <source_dir|file> <target_dir>
  Perform recognition via processTextField call

ConsoleTest.exe --asFields <source_file> <settings.xml> <target_dir>
  Perform recognition via processFields call. Processing settings should be specified in xml file.
          
ConsoleTest.exe --captureData <source_file> <template name> <target_dir>
  Perform data capture for specific document types. Possible template names are:
    * MRZ - perform Machine-Readable Zone extraction

Common options description:
--lang=<languages>: Recognize with specified language. Examples: --lang=English --lang=English,German,French
--out=<output format>: Create output in specified format: txt, rtf, docx, xlsx, pptx, pdfSearchable, pdfTextAndImages, xml
--options=<string>: Pass additional arguments to RESTful calls
");    
        }

        static void Main(string[] args)
        {
            try
            {

                Test tester = new Test();

                ProcessingModeEnum processingMode = ProcessingModeEnum.SinglePage;

                string outFormat = null;
                string language = "english";
                string customOptions = "";

                var p = new OptionSet() {
                { "asDocument", v => processingMode = ProcessingModeEnum.MultiPage },
                { "asTextField", v => processingMode = ProcessingModeEnum.ProcessTextField},
                { "asFields", v => processingMode = ProcessingModeEnum.ProcessFields},
                { "captureData", v => processingMode = ProcessingModeEnum.CaptureData},
                { "out=", (string v) => outFormat = v },
                { "lang=", (string v) => language = v },
                { "options=", (string v) => customOptions = v }
            };

                List<string> additionalArgs = null;
                try
                {
                    additionalArgs = p.Parse(args);
                }
                catch (OptionException)
                {
                    Console.WriteLine("Invalid arguments.");
                    displayHelp();
                    return;
                }

                string sourcePath = null;
                string xmlPath = null;
                string targetPath = null;
                string templateName = null;

                switch (processingMode)
                {
                    case ProcessingModeEnum.SinglePage:
                    case ProcessingModeEnum.MultiPage:
                    case ProcessingModeEnum.ProcessTextField:
                        if (additionalArgs.Count != 2)
                        {
                            displayHelp();
                            return;
                        }

                        sourcePath = additionalArgs[0];
                        targetPath = additionalArgs[1];
                        break;

                    case ProcessingModeEnum.ProcessFields:
                        if (additionalArgs.Count != 3)
                        {
                            displayHelp();
                            return;
                        }

                        sourcePath = additionalArgs[0];
                        xmlPath = additionalArgs[1];
                        targetPath = additionalArgs[2];
                        break;

                    case ProcessingModeEnum.CaptureData:
                        if (additionalArgs.Count != 3)
                        {
                            displayHelp();
                            return;
                        }

                        sourcePath = additionalArgs[0];
                        templateName = additionalArgs[1];
                        targetPath = additionalArgs[2];
                        break;
                }

                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                if (String.IsNullOrEmpty(outFormat))
                {
                    if (processingMode == ProcessingModeEnum.ProcessFields ||
                        processingMode == ProcessingModeEnum.ProcessTextField ||
                        processingMode == ProcessingModeEnum.CaptureData)
                        outFormat = "xml";
                    else
                        outFormat = "txt";
                }

                if (outFormat != "xml" &&
                    (processingMode == ProcessingModeEnum.ProcessFields ||
                    processingMode == ProcessingModeEnum.ProcessTextField) ||
                    processingMode == ProcessingModeEnum.CaptureData)
                {
                    Console.WriteLine("Only xml is supported as output format for field-level recognition.");
                    outFormat = "xml";
                }

                if (processingMode == ProcessingModeEnum.SinglePage || processingMode == ProcessingModeEnum.MultiPage)
                {
                    ProcessingSettings settings = buildSettings(language, outFormat);
                    settings.CustomOptions = customOptions;
                    tester.ProcessPath(sourcePath, targetPath, settings, processingMode);
                }
                else if (processingMode == ProcessingModeEnum.ProcessTextField)
                {
                    TextFieldProcessingSettings settings = buildTextFieldSettings(language, customOptions);
                    tester.ProcessPath(sourcePath, targetPath, settings, processingMode);
                }
                else if (processingMode == ProcessingModeEnum.ProcessFields)
                {
                    string outputFilePath = Path.Combine(targetPath, Path.GetFileName(sourcePath) + ".xml");
                    tester.ProcessFields(sourcePath, xmlPath, outputFilePath);
                }
                else if (processingMode == ProcessingModeEnum.CaptureData)
                {
                    string outputFilePath = Path.Combine(targetPath, Path.GetFileName(sourcePath) + ".xml");
                    tester.CaptureData(sourcePath, templateName, outputFilePath);
                }


            }
            catch (Exception e)
            {
                Console.WriteLine("Error: ");
                Console.WriteLine(e.Message);
            }
        }

        private static ProcessingSettings buildSettings(string language, string outputFormat)
        {
            ProcessingSettings settings = new ProcessingSettings();
            settings.SetLanguage( language );
            switch (outputFormat.ToLower())
            {
                case "txt": settings.OutputFormat = OutputFormat.txt; break;
                case "rtf": settings.OutputFormat = OutputFormat.rtf; break;
                case "docx": settings.OutputFormat = OutputFormat.docx; break;
                case "xlsx": settings.OutputFormat = OutputFormat.xlsx; break;
                case "pptx": settings.OutputFormat = OutputFormat.pptx; break;
                case "pdfsearchable": settings.OutputFormat = OutputFormat.pdfSearchable; break;
                case "pdftextandimages": settings.OutputFormat = OutputFormat.pdfTextAndImages; break;
                case "xml": settings.OutputFormat = OutputFormat.xml; break;
                default:
                    throw new ArgumentException("Invalid output format");
            }

            return settings;
        }

        private static TextFieldProcessingSettings buildTextFieldSettings(string language, string customOptions)
        {
            TextFieldProcessingSettings settings = new TextFieldProcessingSettings();
            settings.Language = language;
            settings.CustomOptions = customOptions;
            return settings;
        }
    }

}
