using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless
{
    public interface IRectEngine
    {
        public Task<List<Rectangle>> GenerateRects();
    }
}
