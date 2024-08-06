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

        //TODO: Check if the system can use compute shaders or not.
        MLClient()
        {
            edgeProvider = new EdgeDetectOpenCVSharp();
        }

        private static Rectangle GetClusterRect(Dbscan.Cluster<EdgePt> cluster)
        {
            int xmin = (int)cluster.Objects.Min(p => p.Point.X);
            int ymin = (int)cluster.Objects.Min(p => p.Point.Y);
            int xmax = (int)cluster.Objects.Max(p => p.Point.X);
            int ymax = (int)cluster.Objects.Max(p => p.Point.Y);

            return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public static List<Rectangle> GetBboxes(Bitmap image)
        {
            IEnumerable<EdgePt> edgeEnumerator = Instance.edgeProvider.GetEdges(image);

            var clusters = DbscanRBush.CalculateClusters(
                edgeEnumerator,
                epsilon: 5,
                minimumPointsPerCluster: 5
            );

            List<Rectangle> rects = new List<Rectangle>();
            foreach (var item in clusters.Clusters)
            {
                rects.Add(GetClusterRect(item));
            }

            return rects;
        }
    }
}
