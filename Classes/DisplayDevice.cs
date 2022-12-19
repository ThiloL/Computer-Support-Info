using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Support_Info
{
    class DisplayDevice
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string DisplayString { get; set; }

        public string Monitor { get; set; }

        public DisplayDeviceInfo Info { get; set; }

        public DisplayDevice()
        {
            Info = new DisplayDeviceInfo();
        }

        public override string ToString()
        {
            return $"{DisplayString} | {Monitor} : {Info.PelsWidth}x{Info.PelsHeight} px, {Info.DisplayFrequency} Hz";
        }
    }

    class DisplayDeviceInfo
    {
        public int BitsPerPel { get; set; }

        public int DisplayFrequency { get; set; }

        public int PelsWidth { get; set; }

        public int PelsHeight { get; set; }
    }
}
