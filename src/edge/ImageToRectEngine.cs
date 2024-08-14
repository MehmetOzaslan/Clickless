using Dbscan;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.src.edge
{
    abstract class ImageToRectEngine : IEdgeProvider
    {
        public DetectionSettings detectionSettings = new DetectionSettings() { cannythresh1 = 100, cannythresh2 = 100, epsilon = 5, iterations = 10, m = 10 };

        public abstract IEnumerable<IPointData> GetEdges(Bitmap bitmap);
        public abstract IEnumerable<Rectangle> GetRects(Bitmap bitmap);

        public void SetDetectionSettings(DetectionSettings settings)
        {
            detectionSettings = settings;
        }

        protected static Rectangle GetClusterRect<T>(IEnumerable<T> cluster) where T : IPointData
        {
            int xmin = (int)cluster.Min(p => p.Point.X);
            int ymin = (int)cluster.Min(p => p.Point.Y);
            int xmax = (int)cluster.Max(p => p.Point.X);
            int ymax = (int)cluster.Max(p => p.Point.Y);

            return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
        }


    }
}
