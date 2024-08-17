using System;
using System.Collections.Generic;
using System.Linq;
using static Clickless.MouseController;
using static Util.MathUtil;
using System.Collections.Concurrent;

namespace Clickless
{
    /// <summary>
    /// Tracks the state of the cursor across coordinates. 
    /// Adds different states
    /// </summary>
    public class CursorStateTracker : ICursorUpdator
    {
        //private Dictionary<Vector2, IntPtr> _cursorPositionStates;
        private ConcurrentDictionary<Vector2, IntPtr> _cursorPositionStates;
        private int _initialCapacity = 2000;
        private int concurrencyLevel = 5;


        public CursorStateTracker() {
            _cursorPositionStates = new ConcurrentDictionary<Vector2, IntPtr>(concurrencyLevel, _initialCapacity);
        }


        public Dictionary<Vector2, IntPtr> GetPositionStates()
        {
            return _cursorPositionStates.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); 
        }

        public void Update(Vector2 pos, CURSORINFO info)
        {
            _cursorPositionStates.TryAdd(pos, info.hCursor);
        }
    }
}
