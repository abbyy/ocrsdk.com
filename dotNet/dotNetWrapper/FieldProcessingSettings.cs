using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AbbyyOnlineSdk
{
    interface IFieldProcessingSettings
    {
        string AsUrlParams { get; }
    }

    public class TextFieldProcessingSettings : IFieldProcessingSettings
    {
        public string AsUrlParams
        {
            get { return "?language=english&textType=normal,handprinted"; }
        }
    }

    public class BarcodeFieldProcessingSettings : IFieldProcessingSettings
    {
        public string AsUrlParams
        {
            get { return ""; }
        }
    }

    public class CheckmarkFieldProcessingSettings : IFieldProcessingSettings
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
}
