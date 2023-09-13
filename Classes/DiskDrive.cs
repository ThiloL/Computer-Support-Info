using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Humanizer;

namespace Computer_Support_Info
{
    class DiskDrive
    {
        public int Index { get; set; }
        public string Caption { get; set; }
        public string SerialNumber { get; set; }
        public Int64 Size { get; set;}

        public string Firmware { get; set; }

        public string DiskSizeText { 
            get {
                return Size.Bytes().Humanize("#.#");
            } 
        }

        public override string ToString()
        {
            return string.Format("{0} | {1} | Serien-Nr.: {2} | Firmware: {3} | Größe: {4}", Index, Caption.Trim(), SerialNumber.Trim(), Firmware.Trim(), DiskSizeText);
        }

    }
}
