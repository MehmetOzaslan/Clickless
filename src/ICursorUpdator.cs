using Util;
using static Clickless.MouseUtilities;

namespace Clickless
{
    internal interface ICursorUpdator
    {
        void Update(MathUtil.Vector2 pos, CURSORINFO info);
    }
}
