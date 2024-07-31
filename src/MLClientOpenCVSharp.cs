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

            public Dbscan.Point Point => new Dbscan.Point(x,y);
        }

        private static Rectangle GetClusterRect(Dbscan.Cluster<EdgePt> cluster)
        {
            int xmin = (int) cluster.Objects.Min(p => p.Point.X);
            int ymin = (int) cluster.Objects.Min(p => p.Point.Y);
            int xmax = (int) cluster.Objects.Max(p => p.Point.X);
            int ymax = (int) cluster.Objects.Max(p => p.Point.Y);

            return new Rectangle(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public static List<Rectangle> GetBboxes(Bitmap image)
        {
            return GetBboxes(BitmapToMat(image));
        }



        public static List<Rectangle> GetBboxes(Mat image)
        {
            int blur = 0;
            int dbDist = 5;

            //NOTE: this is 0.5 to allow for easy bitshifting
            double scaleFactor = 0.5;

            Mat resizedImage = new Mat();
            Cv2.Resize(image, resizedImage, new OpenCvSharp.Size(), scaleFactor, scaleFactor);

            Mat blurredImage = new Mat();
            Cv2.GaussianBlur(resizedImage, blurredImage, new OpenCvSharp.Size(5, 5), 0);

            Mat grayImage = new Mat();
            Cv2.CvtColor(blurredImage, grayImage, ColorConversionCodes.RGB2GRAY);

            Mat edges = new Mat();
            Cv2.Canny(grayImage, edges, 100, 200);

            Cv2.ImShow("Canny Image", edges);


            image.Release();

            // Find coordinates of edge points
            var edgePoints = new ConcurrentBag<EdgePt>();

            Parallel.For(0, edges.Rows, y =>
            {
                for (int x = 0; x < edges.Cols; x++)
                {
                    if (edges.At<byte>(y, x) > 0)
                    {
                        // Rescale the points.
                        edgePoints.Add(new EdgePt(x << 1, y << 1));
                    }
                }
            });


            var clusters =DbscanRBush.CalculateClusters(
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
