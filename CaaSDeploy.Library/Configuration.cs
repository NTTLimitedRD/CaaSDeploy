using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaasDeploy.Library
{
    static class Configuration
    {
        // Move this to config...
        public static Dictionary<string, string> ApiBaseUrls
        {
            get
            {
                return new Dictionary<string, string>
                {
                    { "NA", "https://api-na.dimensiondata.com" },
                    { "EU", "https://api-eu.dimensiondata.com" },
                    { "AU", "https://api-au.dimensiondata.com" },
                    { "AF", "https://api-mea.dimensiondata.com" },
                    { "AP", "https://api-ap.dimensiondata.com" },
                    { "SA", "https://api-latam.dimensiondata.com" },
                    { "CA", "https://api-canada.dimensiondata.com" },
                    { "CANBERRA", "https://api-canberra.dimensiondata.com" },
                };
            }
        }

    }
}
