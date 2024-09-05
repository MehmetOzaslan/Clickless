using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless
{
    public class GridClient : IRectEngine
    {
        public async Task<List<Rectangle>> GenerateRects()
        {
            int margins = 0;
            int spacing = 100;

            var task = Task.Run(() => MonitorUtilities.ObtainRectGrid(margins, spacing));
            return await task;
        }
    }
}
