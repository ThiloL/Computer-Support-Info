using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Computer_Support_Info
{
    class ResolutionAndFramerate
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Framerate { get; set; }

        public override string ToString()
        {
            return string.Format("{0} x {1} @ {2}",
                Width.ToString(),
                Height.ToString(),
                Framerate.ToString()
                );
        }
    }
    class WebcamInfo
    {
        public string Name { get; set; }

        public List<ResolutionAndFramerate> Resolutions { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", Name);
        }

        public string ResultionText
        {
            get
            {
                return string.Join("\n", Resolutions.Select(x => x.ToString()));
            }
        }
    }
}
