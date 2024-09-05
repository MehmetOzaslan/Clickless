using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Clickless
{
    public class MLClient : IRectEngine
    {
        ImageToRectEngine Engine;

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
            if (ImageRectDetectComputeShader.DeviceSupportsCompute()){
                Engine = new ImageRectDetectComputeShader();
            }
            else{
                Console.WriteLine("Compute Shader not supported, switching to CPU implementation");
                Engine = new ImageRectDetectOpenCVSharp();
            }
        }

        public async Task<List<Rectangle>> GenerateRects()
        {
            Bitmap img = MonitorUtilities.CaptureDesktopBitmap();
            var bboxes = await Task.Run(() => GetBboxes(img));
            img.Dispose();
            return bboxes;
        }

        public static void UpdateSettings(DetectionSettings settings)
        {
            Instance.Engine.SetDetectionSettings(settings);
        }

        public static List<Rectangle> GetBboxes(Bitmap image)
        {
            var rects = Instance.Engine.GetRects(image);
            return rects;
        }

        public static Bitmap[] GetEngineImagePasses()
        {
            return Instance.Engine.GetImagePasses();
        }


    }
}
