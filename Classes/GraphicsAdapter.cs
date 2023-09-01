using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Support_Info
{
    class GraphicsAdapter
    {
        public string Name { get; set; }
        //public string DriverVersion { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", Name);

        }
    }
}
