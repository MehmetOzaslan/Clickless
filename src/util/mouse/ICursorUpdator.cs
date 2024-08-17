using Clickless;
using static Clickless.MouseUtilities;

namespace Clickless
{
    internal interface ICursorUpdator
    {
        void Update(MathUtilities.Vector2 pos, CURSORINFO info);
    }
}
