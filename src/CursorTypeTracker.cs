using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;
using static Clickless.MouseController;

namespace Clickless.src
{
    internal class CursorTypeTracker : ICursorUpdator
    {
        private HashSet<CURSORINFO> _cursorTypes;
        public CursorTypeTracker() {
            _cursorTypes = new HashSet<CURSORINFO>();
        }

        public void Update(MathUtil.Vector2 pos, CURSORINFO info)
        {
            _cursorTypes.Add(info);
        }

        public HashSet<CURSORINFO> GetTypes()
        {
            return _cursorTypes;
        }

    }
}
