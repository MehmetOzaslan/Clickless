namespace Clickless.src
{
    public partial class MLClientOpenCVSharp
    {
        public struct EdgePt : Dbscan.IPointData
        {
            double x;
            double y;
            public EdgePt(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public Dbscan.Point Point => new Dbscan.Point(x, y);
        }
    }
}
