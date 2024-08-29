using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless
{
    public class RectangleOverlapRemover
    {
        public static List<Rectangle> RemoveContainingRectangles(List<Rectangle> rectangles)
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
                    if (rectB.Contains(rectA))
                    {
                        overlappingRectangles.Add(rectB);
                    }
                }
            }

            //Return the rects.
            return rectangles.Except(overlappingRectangles).ToList();
        }
    }
}
