using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace Clickless
{
    public class MLClient
    {
        ImageToRectEngine Engine;
        DetectionSettings detectionSettings;

        private static MLClient _instance;
        public static MLClient Instance
        {
            get
            {
                    return _instance ?? (_instance = new MLClient());
            }
            private set
            {
                    _instance = value;
            }
        }

        MLClient()
        {
            detectionSettings = new DetectionSettings() { m = 20,
                cannythresh1 = 100,
                cannythresh2 = 200,
                iterations = 20,
                epsilon = 5,
                minimumArea = 100,
                minimumHeight = 5,
                minimumWidth = 5};

            if (ImageRectDetectComputeShader.DeviceSupportsCompute()){
                Engine = new ImageRectDetectComputeShader();
            }
            else{
                Console.WriteLine("Compute Shader not supported, switching to CPU implementation");
                Engine = new ImageRectDetectOpenCVSharp();
            }
        }



        public static void UpdateSettings(DetectionSettings settings)
        {
            Instance.Engine.SetDetectionSettings(settings);
        }

        public static List<Rectangle> GetBboxes(Bitmap image)
        {
            Instance.Engine.SetDetectionSettings(Instance.detectionSettings);
            var rects = Instance.Engine.GetRects(image).ToList();
            Instance.Engine.FilterRects(rects);
            return rects;
        }

        public static Bitmap[] GetEngineImagePasses()
        {
            return Instance.Engine.GetImagePasses();
        }
    }
}
