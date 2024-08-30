using Dbscan;
using Dbscan.RBush;
using OpenCvSharp;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace Clickless
{
    class ImageRectDetectOpenCVSharp : ImageToRectEngine
    {
        //Members held statically to reduce garbage collection.
        private static Mat resizedImage = new Mat();
        private static Mat blurredImage = new Mat();
        private static Mat grayImage = new Mat();
        private static Mat edges = new Mat();
        private static MatEnumerable edgeEnumerator = new MatEnumerable(edges);

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
            int numberOfChannels = bitmap.PixelFormat == PixelFormat.Format24bppRgb ? 3 : 4;

            // Create a new Mat from the bitmap data
            Mat mat = Mat.FromPixelData(bitmap.Height, bitmap.Width, MatType.CV_8UC(numberOfChannels), bitmapData.Scan0, bitmapData.Stride);

            // Unlock the bits
            bitmap.UnlockBits(bitmapData);

            return mat;
        }

        public override IEnumerable<IPointData> GetEdges(Bitmap bitmap)
        {   
            //0.5 to allow for easy bitshifting.
            double scaleFactor = 0.5;

            var image = BitmapToMat(bitmap);

            Cv2.Resize(image, resizedImage, new OpenCvSharp.Size(), scaleFactor, scaleFactor);
            Cv2.CvtColor(resizedImage, grayImage, ColorConversionCodes.RGB2GRAY);
            Cv2.Canny(grayImage, edges, detectionSettings.cannythresh1, detectionSettings.cannythresh2);

            return new MatEnumerable(edges);
        }

        protected override Bitmap[] GetImagePasses()
        {
            return null;
        }

        protected override IEnumerable<Rectangle> GenerateRects(Bitmap bitmap)
        {
            var edgeEnumerator = GetEdges(bitmap);

            var clusters = DbscanRBush.CalculateClusters(
                edgeEnumerator,
                epsilon: detectionSettings.epsilon,
                minimumPointsPerCluster: detectionSettings.m
            );

            List<Rectangle> rects = new List<Rectangle>();
            foreach (var item in clusters.Clusters)
            {
                rects.Add(GetClusterRect(item.Objects));
            }

            return rects;
        }
    }
}
