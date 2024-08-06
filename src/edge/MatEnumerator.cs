using Dbscan;
using OpenCvSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace Clickless.src
{

    /// <summary>
    /// Enumerator intended to decrease the amount of 
    /// </summary>
    public class MatEnumerator : IEnumerator<IPointData>
    {
        private Mat _mat;
        private int _index;

        private int _currentX { get { return _index % _mat.Cols; }  }
        private int _currentY { get { return _index / _mat.Cols; } }

        public MatEnumerator(Mat mat)
        {
            _mat = mat ?? throw new ArgumentNullException(nameof(mat));
            _index = -1;
        }

        public IPointData Current
        {
            get
            {
                return new EdgePt(_currentX << 1, _currentY << 1);
            }
        }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            //Moves to the next item in the image which is non white.
            while (true)
            {
                _index++;

                if (_index >= _mat.Rows * _mat.Cols)
                    return false;

                if (_mat.At<byte>(_currentY, _currentX) > 0)
                    return true;
            }
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose()
        {
        }
    }

    public class MatEnumerable : IEnumerable<IPointData>
    {
        private Mat _mat;

        public MatEnumerable(Mat mat)
        {
            _mat = mat ?? throw new ArgumentNullException(nameof(mat));
        }

        public IEnumerator<IPointData> GetEnumerator()
        {
            return new MatEnumerator(_mat);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
