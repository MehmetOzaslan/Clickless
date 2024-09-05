using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.MVVM.Models
{
    [AddINotifyPropertyChangedInterface]
    public class GridSettingsModel : IFileNameProvider
    {
        public static readonly string settingsFile = "gridsettings.json";
        public string GetFileName()
        {
            return settingsFile;
        }

        public GridSettingsModel()
        {
            recursionCount = 2;
            n = 4;
            cellSize = 100;
        }

        //How many times the user has to enter in a sequence of commands, must be >= 0
        public int recursionCount { get; set; }

        //How many sub-rects are created in an nxn cube, must be >= 2
        public int n { get; set; }

        //The size of each sub rect
        public int cellSize { get; set; }
    }
}
