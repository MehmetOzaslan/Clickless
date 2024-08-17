﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Clickless
{
    /// <summary>
    /// Ensures that the dictionary is consistent whenever we put in a hashset. 
    /// </summary>
    public class KeySetComparer : IEqualityComparer<HashSet<Keys>>
    {
        public bool Equals(HashSet<Keys> x, HashSet<Keys> y)
        {
            return x.SetEquals(y);
        }

        public int GetHashCode(HashSet<Keys> obj)
        {
            int hash = 19;
            foreach (var key in obj.OrderBy(k => k))
            {
                hash = hash * 31 + key.GetHashCode();
            }
            return hash;
        }
    }
}
