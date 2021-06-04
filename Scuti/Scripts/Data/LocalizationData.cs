using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scuti
{
    public class LocalizationData
    {

        public static Data Localization;

        public class Data
        {
            public string status;
            public string country;
            public string countryCode;
            public string region;
            public string regionName;
            public string city;
            public string zip;
            public float lat;
            public float lon;
            public string timezone;
            public string isp;
            public string org;
            //public string as;
            public string query;
        }
    }
}
