using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.MVVM.Models
{
    public class KeyboardSettingsModel
    {
        public enum KeyboardSpan
        {
            LEFT,
            ALL,
            RIGHT,
            CUSTOM
        }

        KeyboardSpan _chosenSetting {  get; set; }
    }
}
