using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Support_Info
{
    class AudioDevice
    {
        public string Name { get; set; }

        public int VolumeLevel { get; set; }

        public override string ToString()
        {
            return string.Format("{0} | Vol.: {1}", Name, VolumeLevel.ToString());
        }
    }
}
