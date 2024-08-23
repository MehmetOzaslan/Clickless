using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless
{
    

    class DetectionSettingsModel
    {
        public static readonly string settingsFile = "detectionsettings.json";
        public int m { get; set; }
        public int minimumarea { get; set; }
        public int epsilon { get; set; }
        public int iterations { get; set; }
        public int cannythresh1 { get; set; }
        public int cannythresh2 { get; set; }
    }
}
