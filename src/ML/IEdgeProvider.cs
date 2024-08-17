using Dbscan;
using System.Collections.Generic;
using System.Drawing;

namespace Clickless
{
    public interface IEdgeProvider
    {
        IEnumerable<IPointData> GetEdges(Bitmap bitmap);
    }
}
