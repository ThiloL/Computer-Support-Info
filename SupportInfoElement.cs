using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace Computer_Support_Info
{
    public class SupportInfoElement
    {
        public int Column { get; set; }
        public int Number { get; set; }
        public int SubNumber { get; set; }
        public string NumberText { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool MakeBold { get; set; }

        public SupportInfoElement()
        {
            NumberText = string.Empty;
        }
    }
}
