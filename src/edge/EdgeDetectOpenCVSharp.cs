using Dbscan;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Clickless.src.MLClient;

namespace Clickless.src
{
    class EdgeDetectOpenCVSharp : IEdgeProvider
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

        public IEnumerable<IPointData> GetEdges(Bitmap bitmap)
        {
            int blur = 0;
            int dbDist = 5;
            int gaussianKernalSize = 5;
            int cannyThresh1 = 100;
            int cannyThresh2 = 200;

            //0.5 to allow for easy bitshifting.
            double scaleFactor = 0.5;

            var image = BitmapToMat(bitmap);

            Cv2.Resize(image, resizedImage, new OpenCvSharp.Size(), scaleFactor, scaleFactor);
            //Cv2.GaussianBlur(resizedImage, blurredImage, new OpenCvSharp.Size(gaussianKernalSize, gaussianKernalSize), blur);
            Cv2.CvtColor(resizedImage, grayImage, ColorConversionCodes.RGB2GRAY);

            //TODO: Find a way to optimize this, either through the GPU or a different call.
            Cv2.Canny(grayImage, edges, cannyThresh1, cannyThresh2);

            return new MatEnumerable(edges);
        }

    }
}
