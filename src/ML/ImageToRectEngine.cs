using Dbscan;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Clickless
{
    abstract class ImageToRectEngine : IEdgeProvider
    {
        public DetectionSettings detectionSettings = new DetectionSettings();
        private List<Rectangle> _rectsDetected;
        protected abstract Bitmap[] GetImagePasses();
        public abstract IEnumerable<IPointData> GetEdges(Bitmap bitmap);
        protected abstract IEnumerable<Rectangle> GenerateRects(Bitmap bitmap);
        public Bitmap[] GetImagePasses(bool showRects = true){
            Bitmap[] passes = GetImagePasses();
            if (showRects)
            {
                foreach (Bitmap bitmap in passes) {
                    ApplyRectResultToBmp(_rectsDetected, bitmap);
                }
            }
            return passes;
        }
        public List<Rectangle> GetRects(Bitmap bitmap){
            List<Rectangle> rects = GenerateRects(bitmap).ToList();
            FilterRects(ref rects);
            _rectsDetected = rects;
            return rects;
        }
        public void SetDetectionSettings(DetectionSettings settings){
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
        protected void FilterRects(ref List<Rectangle> rectangles){
            RectangleFilters.RemoveSmallRectangles(ref rectangles, detectionSettings);
            RectangleFilters.RemoveContainingRectangles(ref rectangles);
            RectangleFilters.RemoveRectanglesWithLargeAspectRatio(ref rectangles, detectionSettings);
        }
        public Bitmap ApplyRectResultToBmp(List<Rectangle> rectangles, Bitmap bmp){
            if(rectangles != null && rectangles.Any())
            {
                using (Graphics g = Graphics.FromImage(bmp)) { 
                    g.DrawRectangles(new (new SolidBrush(Color.FromArgb(180, 255,0,0 ) ), 4), rectangles.ToArray());
                }
            }
            return bmp;
        }
    }
}
