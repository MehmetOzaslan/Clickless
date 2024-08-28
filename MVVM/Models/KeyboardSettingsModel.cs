using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.MVVM.Models
{
    public class KeyboardSettingsModel : IFileNameProvider
    {
        public static readonly string settingsFile = "keyboardsettings.json";

        public enum KeyboardSpan
        {
            LEFT = 1,
            ALL = 2,
            RIGHT = 3,
            CUSTOM = 4
        }

        public KeyboardSpan chosenSetting {  get; set; }

        public string GetFileName()
        {
            return settingsFile;
        }
    }
}
