using Clickless.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless
{
    public class GridClient : IRectEngine
    {

        public static GridSettingsModel gridSettings = new GridSettingsModel();

        public GridClient()
        {
        }

        public static void UpdateSettings(GridSettingsModel settings)
        {
            gridSettings = settings;
        }

        public async Task<List<Rectangle>> GenerateRects()
        {
            var task = Task.Run(() => MonitorUtilities.ObtainRectGrid(0, gridSettings.cellSize));
            return await task;
        }

        public async Task<List<Rectangle>> GenerateRects(Rectangle rectangle)
        {
            var task = Task.Run(() => RecurseOnGridBox(rectangle, gridSettings.n));
            return await task;
        }


        //Divides up a rect into nxn rects.
        public static List<Rectangle> RecurseOnGridBox(Rectangle rectangle, int n)
        {
            if (n<= 0)
            {
                return new List<Rectangle>() { rectangle };
            }

            List<Rectangle> result = new List<Rectangle>();

            int width = rectangle.Width / n;
            int height = rectangle.Height / n;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    Rectangle r = new Rectangle();
                    r.Width = width;
                    r.Height = height;
                    r.X = width * i + rectangle.X;
                    r.Y = width * j + rectangle.Y;

                    result.Add(r);
                }
            }

            return result;
        }
    }
}
