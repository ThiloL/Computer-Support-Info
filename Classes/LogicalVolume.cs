﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Humanizer;

namespace Computer_Support_Info
{
    class LogicalVolume
    {
        public string Name { get; set; }

        public long TotalSpace { get; set; }

        public long FreeSpace { get; set; }

        public override string ToString()
        {

            return string.Format("Größe: {0} | Frei: {1}",
                TotalSpace.Bytes().Humanize("0.#"),
                FreeSpace.Bytes().Humanize("0.#")
                );

        }

    }
}
