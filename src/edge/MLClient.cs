using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Policy;
using System.Windows.Markup;
using Dbscan.RBush;
using System.Collections.Concurrent;
using System.Collections;
using System.Drawing.Printing;
using System.Threading;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using Dbscan;
using System.Diagnostics;
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

            detectionSettings = new DetectionSettings() {m = 3, cannythresh1 = 100, cannythresh2 = 200, iterations = 100, epsilon = 5 };

            //TODO: Use dx11 to check directly if compute shaders are supported.
            try
            {
                Engine = new ImageRectDetectComputeShader();
            }
            catch (Exception ex) { 

                Console.Error.WriteLine(ex.Message);
                Console.WriteLine("Compute Shader not supported, switching to CPU implementation");
                Engine = new EdgeDetectOpenCVSharp();
            }

        }


        public static List<Rectangle> GetBboxes(Bitmap image)
        {
            IEnumerable<IPointData> edgeEnumerator;

            //try
            //{
            //    edgeEnumerator = Instance.Engine.GetEdges(image);
            //}
            //catch (Exception ex)
            //{
            //    Console.Error.WriteLine(ex.Message);
            //    Console.WriteLine("Compute Shader not supported, switching to CPU implementation");
            //    Instance.Engine = new EdgeDetectOpenCVSharp();

            //    edgeEnumerator = Instance.Engine.GetEdges(image);
            //}
            Instance.Engine.SetDetectionSettings(Instance.detectionSettings);

            return Instance.Engine.GetRects(image).ToList();
        }
    }
}
