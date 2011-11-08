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
            bool processAsDocument = false;
            bool showHelp = false;

            string outFormat = "pdfSearchable";
            string language = "english";

            var p = new OptionSet() {
                { "asDocument", "Process given files as single multi-page document", 
                    v => processAsDocument = true },
                { "out=", "Create output in specified {format}: txt, rtf, docx, xlsx, pptx, pdfSearchable, pdfTextAndImages, xml",
                    (string v) => outFormat = v },
                { "lang=", "Recognize with specified {language}",
                    (string v) => language = v },
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

            ProcessingSettings settings = buildSettings(language, outFormat);

            if (showHelp)
            {
                Console.WriteLine("Process images with ABBYY Cloud OCR SDK");
                Console.WriteLine("Usage:");
                Console.WriteLine("ConsoleTest.exe [options] <input dir|file> <output dir>");
                Console.WriteLine("Options:");
                p.WriteOptionDescriptions(Console.Out);
                return;
            }

            string sourcePath = additionalArgs[0];
            string targetPath = additionalArgs[1];

            try
            {
                Test tester = new Test();
                tester.ProcessPath(sourcePath, targetPath, settings, processAsDocument);

            }
            catch (Abbyy.CloudOcrSdk.ProcessingErrorException e)
            {
                Console.WriteLine("Cannot process.");
                Console.WriteLine(e.ToString());
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
    }

}
