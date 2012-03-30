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
                string result = "?language=" + Language + "&textType=" + TextType.AsUrlParams();
                if (!String.IsNullOrEmpty(CustomOptions))
                {
                    result += "&" + CustomOptions;
                }
                return result;
            }
        }

        public string Language { get; set; }
        public TextType TextType { get; set; }

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
        Handprinted=0x02, // Works only for field-level recognition
        Gothic=0x04
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

            if ( (textTypes & TextType.Normal) == TextType.Normal )
                appendToResult(result, "normal");

            if ((textTypes & TextType.Handprinted) == TextType.Handprinted)
                appendToResult(result, "handprinted");

            if ((textTypes & TextType.Gothic) == TextType.Gothic)
                appendToResult(result, "gothic");

            return result.ToString();
        }
    }
}
