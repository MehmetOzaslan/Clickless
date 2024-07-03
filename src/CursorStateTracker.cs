﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Util;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using static Clickless.MouseController;
using System.IO;
using static Util.MathUtil;

namespace Clickless.src
{
    /// <summary>
    /// Tracks the state of the cursor across coordinates. 
    /// Adds different states
    /// </summary>
    public class  CursorStateTracker : ICursorUpdator
    {
        private Dictionary<MathUtil.Vector2, CURSORINFO> _cursorPositionStates;

        public CursorStateTracker() {
            _cursorPositionStates = new Dictionary<MathUtil.Vector2, CURSORINFO>();
        }

        public Dictionary<MathUtil.Vector2, CURSORINFO> GetPositionStates()
        {
            return _cursorPositionStates; 
        }

        public void Update(MathUtil.Vector2 pos, CURSORINFO info)
        {
            _cursorPositionStates.Add(pos, info);
        }
    }
}
