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

        //Defaults
        public DetectionSettings()
        {
            m = 20;
            cannythresh1 = 100;
            cannythresh2 = 200;
            iterations = 20;
            epsilon = 5;
            minimumRectArea = 20;
            minimumRectHeight = 5;
            minimumRectWidth = 5;
            maximumAspectRatio = 500;
            lowerEdgeDetectionThreshold = 0.001f;
        }

        public int m { get; set; }
        public int minimumRectArea { get; set; }
        public int minimumRectHeight { get; set; }
        public int minimumRectWidth { get; set; }
        public int epsilon { get; set; }
        public int iterations { get; set; }
        public int cannythresh1 { get; set; }
        public int cannythresh2 { get; set; }
        public int maximumAspectRatio { get; set; }
        public float lowerEdgeDetectionThreshold {  get; set; }
    }
}
