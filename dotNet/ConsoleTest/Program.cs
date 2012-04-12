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
       
    

        static void Main(string[] args)
        {

            ProcessingModeEnum processingMode = ProcessingModeEnum.SinglePage;
            bool showHelp = false;

            string outFormat = null;
            string language = "english";
            string customOptions = "";

            var p = new OptionSet() {
                { "asDocument", "Process given files using submitImage/processDocument", 
                    v => processingMode = ProcessingModeEnum.MultiPage },
                { "asTextField", "Process file using processTextField", 
                    v => processingMode = ProcessingModeEnum.ProcessTextField},
                { "out=", "Create output in specified {format}: txt, rtf, docx, xlsx, pptx, pdfSearchable, pdfTextAndImages, xml",
                    (string v) => outFormat = v },
                { "lang=", "Recognize with specified {language}",
                    (string v) => language = v },
                { "options=", "Recognize with specified custom options",
                    (string v) => customOptions = v },
                { "h|help", "Show this message and exit", 
                     v => showHelp = v != null }
            };

            List<string> additionalArgs = null;
            try
            {
                additionalArgs = p.Parse(args);
            }
            catch (OptionException)
            {
                Console.WriteLine("Invalid arguments.");
                showHelp = true;
            }

            if (additionalArgs != null && additionalArgs.Count != 2)
            {
                showHelp = true;
            }

            if (showHelp)
            {
                Console.WriteLine("Process images with ABBYY Cloud OCR SDK");
                Console.WriteLine("Usage:");
                Console.WriteLine("ConsoleTest.exe [options] <input dir|file> <output dir>");
                Console.WriteLine("Options:");
                p.WriteOptionDescriptions(Console.Out);
                return;
            }

            if (String.IsNullOrEmpty(outFormat))
            {
                outFormat = "txt";
            }
            else if (processingMode == ProcessingModeEnum.ProcessTextField)
            {
                Console.WriteLine("Only xml is supported as output format for field-level recognition.");
            }

            string sourcePath = additionalArgs[0];
            string targetPath = additionalArgs[1];

            try
            {
                Test tester = new Test();

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
