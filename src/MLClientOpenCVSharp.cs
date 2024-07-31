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

namespace Clickless.src
{
    public class MLClientOpenCVSharp
    {
        /// <summary>
        /// Converts a bitmap into a map.
        /// NOTE: The data provided is not automatically de-allocated by opencvsharp, so it must be taken care of.
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static Mat BitmapToMat(Bitmap bitmap)
        {


            // Lock the bitmap's bits
            BitmapData bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat);

            // Define the number of channels based on the PixelFormat
            int numberOfChannels = (bitmap.PixelFormat == PixelFormat.Format24bppRgb) ? 3 : 4;

            // Create a new Mat from the bitmap data
            Mat mat = Mat.FromPixelData(bitmap.Height, bitmap.Width, MatType.CV_8UC(numberOfChannels), bitmapData.Scan0, bitmapData.Stride);

            // Unlock the bits
            bitmap.UnlockBits(bitmapData);

            return mat;
        }


        private struct EdgePt : Dbscan.IPointData
        {
            double x;
            double y;

            public EdgePt(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public Dbscan.Point Point => new Dbscan.Point(x, y);
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
            return GetBboxes(BitmapToMat(image));
        }

        private static Mat resizedImage = new Mat();
        private static Mat blurredImage = new Mat();
        private static Mat grayImage = new Mat();
        private static Mat edges = new Mat();

        public static List<Rectangle> GetBboxes(Mat image)
        {
            int blur = 0;
            int dbDist = 5;
            int gaussianKernalSize = 5;
            int cannyThresh1 = 100;
            int cannyThresh2 = 200;

            //NOTE: this is 0.5 to allow for easy bitshifting
            double scaleFactor = 0.5;

            Cv2.Resize(image, resizedImage, new OpenCvSharp.Size(), scaleFactor, scaleFactor);

            Cv2.GaussianBlur(resizedImage, blurredImage, new OpenCvSharp.Size(gaussianKernalSize, gaussianKernalSize), 0);

            Cv2.CvtColor(blurredImage, grayImage, ColorConversionCodes.RGB2GRAY);

            Cv2.Canny(grayImage, edges, cannyThresh1, cannyThresh2);


            var edgePoints = new ConcurrentBag<EdgePt>();
            // Find coordinates of edge points
            Parallel.For(0, edges.Rows * edges.Cols, i =>
            {
                int y = i / edges.Cols;
                int x = i % edges.Cols;
                
                if (edges.At<byte>(y, x) > 0)
                {
                    // Rescale the 2x points via bitshift
                    edgePoints.Add(new EdgePt(x << 1, y << 1));
                }

            });


            var clusters = DbscanRBush.CalculateClusters(
                edgePoints,
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
