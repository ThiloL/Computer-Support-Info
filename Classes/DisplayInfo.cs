using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Support_Info
{
    class DisplayInfo
    {
        public string Manufacturer { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Manufacturer)) return Name.Trim();
            return string.Format("{0} {1}", Manufacturer.Trim(), Name.Trim());
        }
    }
}
