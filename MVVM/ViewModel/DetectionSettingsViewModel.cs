using Clickless.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing.Printing;

namespace Clickless.MVVM.ViewModel
{
    class DetectionSettingsViewModel : ObservableObject
    {
        private DetectionSettings _detectionSettings;
        public RelayCommand SaveSettingsCommand;

        public DetectionSettings DetectionSettings
        {
            get => _detectionSettings;
            set
            {
                _detectionSettings = value;
                OnPropertyChanged();
            }
        }

        public int M
        {
            get => DetectionSettings.m;
            set
            {
                if (DetectionSettings.m != value)
                {
                    DetectionSettings.m = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public int Epsilon
        {
            get => DetectionSettings.epsilon;
            set
            {
                if (DetectionSettings.epsilon != value)
                {
                    DetectionSettings.epsilon = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public int Iterations
        {
            get => DetectionSettings.iterations;
            set
            {
                if (DetectionSettings.iterations != value)
                {
                    DetectionSettings.iterations = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public int CannyThresh1
        {
            get => DetectionSettings.cannythresh1;
            set
            {
                if (DetectionSettings.cannythresh1 != value)
                {
                    DetectionSettings.cannythresh1 = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public int CannyThresh2
        {
            get => DetectionSettings.cannythresh2;
            set
            {
                if (DetectionSettings.cannythresh2 != value)
                {
                    DetectionSettings.cannythresh2 = value;
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public DetectionSettingsViewModel()
        {
            DetectionSettings = LoadSettings();
            SaveSettingsCommand = new RelayCommand((obj) => { SaveSettings(); }, (obj) => { return true; });
        }

        private DetectionSettings LoadSettings()
        {
            return ObjectSerializer.LoadDataOrDefault<DetectionSettings>()
                   ?? new DetectionSettings()
                            {
                                m = 20,
                                cannythresh1 = 100,
                                cannythresh2 = 200,
                                iterations = 20,
                                epsilon = 5,
                            };
        }

        private void SaveSettings()
        {  
            ObjectSerializer.SaveData(DetectionSettings, DetectionSettings.settingsFile);
        }
    }
}
