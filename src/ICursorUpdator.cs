using Util;
using static Clickless.MouseController;

namespace Clickless
{
    internal interface ICursorUpdator
    {
        void Update(MathUtil.Vector2 pos, CURSORINFO info);
    }
}
