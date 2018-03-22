using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample
{
    public enum CountryOfOrigin
    {
        Australia,
        Brazil,
        Canada,
        China,
        France,
        Germany,
        Italy,
        Japan,
        Korea,
        Netherlands,
        Poland,
        Russia,
        Singapore,
        Spain,
        Taiwan,
        Turkey,
        UK,
        Usa,
    };

    public class ProcessingSettings
    {
        public ProcessingSettings()
        {
            Country = CountryOfOrigin.Usa;
            TreatAsPhoto = true;
        }

        public CountryOfOrigin Country
        {
            set { country = value.ToString(); }
        }

        public void SetCountry(string value)
        {
            country = value;
        }

        public bool TreatAsPhoto { get; set; }

        public string AsUrlParams
        {
            get
            {
                StringBuilder result = new StringBuilder();

                result.Append("exportFormat=xml");
                result.Append("&country=" + country);
                string imageSource = null;
                if (TreatAsPhoto)
                {
                    imageSource = "photo";
                }
                else
                {
                    imageSource = "scanner";
                }
                result.Append( "&imageSource=" + imageSource );
                return result.ToString();
            }
        }

        private string country = "usa";
    }
}
