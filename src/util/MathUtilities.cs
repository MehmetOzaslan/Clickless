using System;
using System.Drawing;

namespace Clickless
{
    public class MathUtilities
    {
        public struct Vector2
        {
            public float x, y;

            public Vector2(float x, float y) : this()
            {
                this.x = x;
                this.y = y;
            }
            public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        }

        /// <summary>
        /// t is not manually bound from 0 to 1 and will not throw an error.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float Lerp(float a, float b, float t) { return a + (b - a) * t; }

        /// <summary>
        /// t is not manually bound from 0 to 1 and will not throw an error.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector2 lerp(Vector2 a, Vector2 b, float t) { return new Vector2(Lerp(a.x,b.x,t), Lerp(a.y,b.y,t)); }

        public static Vector2[] GenerateCirclePoints(int numPoints, float radius = 1f, float startingOffset = 0f)
        {
            var points = new Vector2[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                float t = (float)i / (numPoints - 1);
                float x = (float)Math.Cos(2 * Math.PI * t + startingOffset) * radius;
                float y = (float)Math.Sin(2 * Math.PI * t + startingOffset) * radius;

                points[i] = new Vector2(x,y);
            }
            return points;
        }

        public static Rectangle GenerateRectFromPoint(Vector2 point, int h, int w)
        {
            return new Rectangle((int)(point.x - w / 2), (int)(point.y - h / 2), h, w);
        }

    }
}
