using Clickless.MVVM.Models;

namespace Clickless
{
    public class DetectionSettings : IFileNameProvider
    {
        public static readonly string settingsFile = "detectionsettings.json";
        public string GetFileName()
        {
            return settingsFile;
        }

        public int m { get; set; }
        public int minimumArea { get; set; }
        public int minimumHeight { get; set; }
        public int minimumWidth { get; set; }
        public int epsilon { get; set; }
        public int iterations { get; set; }
        public int cannythresh1 { get; set; }
        public int cannythresh2 { get; set; }
    }
}
