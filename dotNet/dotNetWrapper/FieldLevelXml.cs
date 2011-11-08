using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Abbyy.CloudOcrSdk
{
    public class FieldLevelXml
    {
        /// <summary>
        /// Read text result from field-level xml
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadText(string filePath)
        {
            XDocument xDoc = XDocument.Load(filePath);
            var ns = xDoc.Root.GetDefaultNamespace();
            var field = xDoc.Root.Element(ns+"field");
            
            var value = field.Element(ns+"value");
            string result = value.Value;

            var xEncoding = value.Attribute("encoding");
            if (xEncoding != null && xEncoding.Value.ToLower() == "base64")
            {
                byte[] bytes = Convert.FromBase64String(result);

                System.Text.Encoding encoding = new System.Text.UnicodeEncoding();
                result = encoding.GetString(bytes);
            }

            return result;
        }
    }
}
