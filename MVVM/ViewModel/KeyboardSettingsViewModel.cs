using Clickless.Core;
using Clickless.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using static Clickless.MVVM.Models.KeyboardSettingsModel;

namespace Clickless.MVVM.ViewModel
{

    class KeyboardSettingsViewModel : ObservableObject
    {
        static readonly char[] _allchars = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        static readonly char[] _lhsChars = { 'Q', 'W', 'E', 'R', 'T', 'A', 'S', 'D', 'F', 'G', 'Z', 'X', 'C', 'V', 'B' };

        static readonly char[] _rhsChars = { 'Y', 'U', 'I', 'O', 'P', 'H', 'J', 'K', 'L', 'N', 'M' };


        public KeyboardSettingsModel keyboardSettingsModel;

        private KeyboardSettingsModel LoadSettings()
        {
            return ObjectSerializer.LoadDataOrDefault<KeyboardSettingsModel>(settingsFile)
                   ?? new KeyboardSettingsModel()
                   {
                       chosenSetting = KeyboardSpan.ALL,
                   };
        }

        private void SaveSettings()
        {
            ObjectSerializer.SaveData(keyboardSettingsModel, settingsFile);
        }

        private KeyboardSpan _selectedSpan;
        public KeyboardSpan SelectedSpan
        {
            get => _selectedSpan;
            set
            {
                if (_selectedSpan != value)
                {
                    _selectedSpan = value;
                    keyboardSettingsModel.chosenSetting = value;
                    SaveSettings();
                    OnPropertyChanged(nameof(SelectedSpan));
                }
            }
        }

        public KeyboardSettingsViewModel()
        {
            keyboardSettingsModel = LoadSettings();
            SelectedSpan = keyboardSettingsModel.chosenSetting;
        }

    }
}
