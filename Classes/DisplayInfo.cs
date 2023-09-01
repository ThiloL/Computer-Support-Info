using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Support_Info
{
    class DisplayInfo
    {
        public string Name { get; set; }

        public int ResX { get; set; }

        public int ResY { get; set; }

        public int Frequency { get; set; }

        public string Dpi { get; set; }


        public override string ToString()
        {
            return string.Format("{0}, {1}x{2} px, {3} Hz, {4}", 
                Name.Trim()
                ,ResX.ToString()
                ,ResY.ToString()
                ,Frequency.ToString()
                ,Dpi
            );
        }
    }
}
