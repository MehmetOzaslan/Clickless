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

namespace Clickless.src
{
    public class MLClient
    {
        IEdgeProvider edgeProvider;

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
            try
            {
                edgeProvider = new EdgeDetectComputeShader();
            }
            catch (Exception ex) { 

                Console.Error.WriteLine(ex.Message);
                Console.WriteLine("Compute Shader not supported, switching to CPU implementation");
                edgeProvider = new EdgeDetectOpenCVSharp();
            }

        }


        private static Rectangle GetClusterRect(Dbscan.Cluster<IPointData> cluster)
        {
            int xmin = (int)cluster.Objects.Min(p => p.Point.X);
            int ymin = (int)cluster.Objects.Min(p => p.Point.Y);
            int xmax = (int)cluster.Objects.Max(p => p.Point.X);
            int ymax = (int)cluster.Objects.Max(p => p.Point.Y);

            return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public static List<Rectangle> GetBboxes(Bitmap image)
        {
            IEnumerable<IPointData> edgeEnumerator;

            try
            {
                edgeEnumerator = Instance.edgeProvider.GetEdges(image);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.WriteLine("Compute Shader not supported, switching to CPU implementation");
                Instance.edgeProvider = new EdgeDetectOpenCVSharp();

                edgeEnumerator = Instance.edgeProvider.GetEdges(image);
            }


            var st = Stopwatch.StartNew();


            var clusters = DbscanRBush.CalculateClusters(
                edgeEnumerator,
                epsilon: 5,
                minimumPointsPerCluster: 5
            );

            st.Stop();

            Console.WriteLine("DBScan took: " + st.ElapsedMilliseconds);

            List<Rectangle> rects = new List<Rectangle>();
            foreach (var item in clusters.Clusters)
            {
                rects.Add(GetClusterRect(item));
            }

            return rects;
        }
    }
}
