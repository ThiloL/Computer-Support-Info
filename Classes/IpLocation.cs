using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Support_Info.Classes
{
    public class IpLocation
    {
        public string country_name { get; set; }
        public string state_prov { get; set; }
        public string city { get; set; }
        public string isp { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (country_name != null) { 
                sb.Append(country_name); 
            }
            
            if (state_prov != null)
            {
                if (sb.Length > 0) { sb.Append(" | ");}
                sb.Append(state_prov);
            }

            if (city != null)
            {
                if (sb.Length > 0) { sb.Append(" | "); }
                sb.Append(city);
            }

            if (city != null)
            {
                if (sb.Length > 0) { sb.Append(" | "); }
                sb.Append(isp);
            }

            return sb.ToString();
        }
    }
}
