using Clickless.MVVM.View;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless
{
    public class RectangleFilters
    {

        public static void RemoveSmallRectangles(ref List<Rectangle> rectangles, DetectionSettings detectionSettings)
        {
            rectangles = rectangles.Where((rect) =>
            {
                return
                    (rect.Width * rect.Height) > detectionSettings.minimumRectArea
                    && rect.Width > detectionSettings.minimumRectWidth
                    && rect.Height > detectionSettings.minimumRectHeight;
            }).ToList();
        }

        public static void RemoveRectanglesWithLargeAspectRatio(ref List<Rectangle> rectangles, DetectionSettings detectionSettings)
        {
            rectangles = rectangles.Where((rect) =>
            {
                return (rect.Width / rect.Height) < detectionSettings.maximumAspectRatio && (rect.Height / rect.Width) < detectionSettings.maximumAspectRatio;
            }).ToList();
        }

        public static void RemoveContainingRectangles(ref List<Rectangle> rectangles)
        {
            var sortedRectangles = rectangles.OrderBy(r => r.Left).ToList();

            HashSet<Rectangle> overlappingRectangles = new HashSet<Rectangle>();

            // Sweep line algorithm to detect overlaps
            for (int i = 0; i < sortedRectangles.Count; i++)
            {
                var rectA = sortedRectangles[i];

                for (int j = i + 1; j < sortedRectangles.Count; j++)
                {
                    var rectB = sortedRectangles[j];

                    //Minor optimization to avoid IntersectsWith.
                    if (rectB.Left > rectA.Right)
                        break;

                    if (rectA.Contains(rectB))
                    {
                        overlappingRectangles.Add(rectA);
                    }
                    else if (rectB.Contains(rectA))
                    {
                        overlappingRectangles.Add(rectB);
                    }

                    //Force the larger rectangle to move out of the way of the smaller rectangle.
                    else if (rectA.IntersectsWith(rectB))
                    {
                        Rectangle largerRect = rectA.Width * rectA.Height > rectB.Width * rectB.Height ? rectA : rectB;
                        Rectangle smallerRect = largerRect == rectA ? rectB : rectA;

                        // Move the larger rectangle out of the way of the smaller rectangle
                        if (largerRect.Left < smallerRect.Left)
                            largerRect.X = smallerRect.Right; // Move to the right
                        else
                            largerRect.X = smallerRect.Left - largerRect.Width; // Move to the left

                        if (largerRect.Top < smallerRect.Top)
                            largerRect.Y = smallerRect.Bottom; // Move below
                        else
                            largerRect.Y = smallerRect.Top - largerRect.Height; // Move above
                    }
                }
            }

            rectangles = rectangles.Except(overlappingRectangles).ToList();
        }
    }
}
