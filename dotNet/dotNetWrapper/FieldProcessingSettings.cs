using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Abbyy.CloudOcrSdk
{
    public interface IProcessingSettings
    {
        string AsUrlParams { get; }
    }

    public class TextFieldProcessingSettings : IProcessingSettings
    {
        public TextFieldProcessingSettings()
        {
            Language = "english";
            TextType = TextType.Normal | TextType.Handprinted;
        }

        public string AsUrlParams
        {
            get
            {
                StringBuilder result = new StringBuilder("?language=" + Language + 
                    "&textType=" + TextType.AsUrlParams());
                if (!String.IsNullOrEmpty(Letterset))
                {
                    result.AppendFormat("&letterSet={0}", Letterset);
                }
                if (!String.IsNullOrEmpty(Regexp))
                {
                    result.AppendFormat("&regExp={0}", Regexp);
                }
                if (!String.IsNullOrEmpty(CustomOptions))
                {
                    result.Append("&" + CustomOptions);
                }
                return result.ToString();
            }
        }

        public string Language { get; set; }
        public TextType TextType { get; set; }
        public string Letterset { get; set; }
        public string Regexp { get; set; }

        public String CustomOptions { get; set; }
    }

    public class BarcodeFieldProcessingSettings : IProcessingSettings
    {
        public string AsUrlParams
        {
            get { return ""; }
        }
    }

    public class CheckmarkFieldProcessingSettings : IProcessingSettings
    {
        public string Params { get; set; }
        public string AsUrlParams
        {
            get
            {
                if( String.IsNullOrEmpty( Params ) )
                    return "";
                else
                    return "?" + Params;
            }
        }
    }

    [Flags]
    public enum TextType
    {
        Normal=0x01, 
        Typewriter=0x02,
        Matrix=0x04,
        Index=0x08,
        Handprinted=0x10, // Works only for field-level recognition
        OcrA = 0x20,
        OcrB = 0x40,
        E13b = 0x80,
        Cmc7 = 0x100,
        Gothic=0x200
    }

    public static class TextTypesExtensions
    {
        private static void appendToResult(StringBuilder result, string value)
        {
            if (result.Length > 0)
                result.Append(",");
            result.Append(value);
        }

        public static string AsUrlParams(this TextType textTypes)
        {
            StringBuilder result = new StringBuilder();

            if ((textTypes & TextType.Normal) != 0)
                appendToResult(result, "normal");

            if ((textTypes & TextType.Typewriter) != 0)
                appendToResult(result, "typewriter");

            if ((textTypes & TextType.Matrix) != 0)
                appendToResult(result, "matrix");

            if ((textTypes & TextType.Index) != 0)
                appendToResult(result, "index");

            if ((textTypes & TextType.Handprinted) != 0)
                appendToResult(result, "handprinted");

            if ((textTypes & TextType.OcrA) != 0)
                appendToResult(result, "ocrA");

            if ((textTypes & TextType.OcrB) != 0)
                appendToResult(result, "ocrB");

            if ((textTypes & TextType.E13b) != 0)
                appendToResult(result, "e13b");

            if ((textTypes & TextType.Cmc7) != 0)
                appendToResult(result, "cmc7");

            if ((textTypes & TextType.Gothic) != 0)
                appendToResult(result, "gothic");

            if (result.Length == 0)
                return "normal";

            return result.ToString();
        }
    }
}
