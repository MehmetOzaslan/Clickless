using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Clickless.src.edge;

namespace Clickless.src
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
            detectionSettings = new DetectionSettings() {m = 3, cannythresh1 = 100, cannythresh2 = 200, iterations = 10, epsilon = 5 };

            if (ImageRectDetectComputeShader.DeviceSupportsCompute()){
                Engine = new ImageRectDetectComputeShader();
            }
            else{
                Console.WriteLine("Compute Shader not supported, switching to CPU implementation");
                Engine = new EdgeDetectOpenCVSharp();
            }
        }


        public static List<Rectangle> GetBboxes(Bitmap image)
        {
            Instance.Engine.SetDetectionSettings(Instance.detectionSettings);
            return Instance.Engine.GetRects(image).ToList();
        }
    }
}
