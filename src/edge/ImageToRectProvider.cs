using Dbscan;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.src.edge
{
    abstract class ImageToRectProvider : IEdgeProvider
    {
        public abstract IEnumerable<IPointData> GetEdges(Bitmap bitmap);
        public abstract IEnumerable<Rectangle> GetRects(Bitmap bitmap);
    }
}
