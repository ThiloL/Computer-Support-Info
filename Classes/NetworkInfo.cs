using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Humanizer;

namespace Computer_Support_Info
{
    class NetworkInfo
    {
        public string AdapterName { get; set; }

        public string IP { get; set; }

        public long Speed { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(AdapterName)) sb.Append(AdapterName);
            if (Speed > 0) sb.Append(string.Format(" | Geschw.: {0} ",Speed.Bits().Per(1.Seconds()).Humanize("0.#")));
            if (!string.IsNullOrWhiteSpace(IP)) sb.Append(string.Format(" | IP: {0}", IP));
            
            return sb.ToString();
        }
    }
}
