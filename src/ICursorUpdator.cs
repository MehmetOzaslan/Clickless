using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;
using static Clickless.MouseController;


namespace Clickless.src
{
    internal interface ICursorUpdator
    {
        void Update(MathUtil.Vector2 pos, CURSORINFO info);
    }
}
