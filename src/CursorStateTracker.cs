using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Util;

namespace Clickless.src
{
    /// <summary>
    /// Tracks the state of the cursor across coordinates. 
    /// Adds different states
    /// </summary>
    public class  CursorStateTracker : ICursorUpdator
    {
        private Dictionary<MathUtil.Vector2, MouseController.CURSORINFO> _cursorPositionStates;

        public CursorStateTracker() {
            _cursorPositionStates = new Dictionary<MathUtil.Vector2, MouseController.CURSORINFO>();
        }

        public void Update(MathUtil.Vector2 pos, MouseController.CURSORINFO info)
        {
            _cursorPositionStates.Add(pos, info);
        }
    }

    /// <summary>
    /// Factory style class. Converts tracked positions into an image for sending to the ML backend.
    /// </summary>
    public class CursorImageGenerator
    {
        int width = 10;
        int height = 10;

        public CursorImageGenerator(CursorStateTracker tracker) {
        }

        //TODO:
    }
}
