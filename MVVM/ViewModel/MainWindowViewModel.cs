using Clickless.Core;
using Clickless.MVVM.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.MVVM.ViewModel
{
    class MainWindowViewModel :	ObservableObject
    {
		public DetectionSettingsViewModel DetectionSettingsVM { get; set; }
        public KeyboardSettingsViewModel KeyboardSettingsVM{ get; set; }


        public RelayCommand DetectionSettingViewCommand { get; set; }

		public RelayCommand KeyboardSettingViewCommand { get; set; }

		private object _currentView;
		public object CurrentView
		{
			get { return _currentView; }
			set { 
				_currentView = value;
				OnPropertyChanged();
			}
		}

		public MainWindowViewModel()
		{
            DetectionSettingsVM = new DetectionSettingsViewModel();
            KeyboardSettingsVM = new KeyboardSettingsViewModel();
            CurrentView = DetectionSettingsVM;

			DetectionSettingViewCommand = new RelayCommand( (o) => { CurrentView = DetectionSettingsVM; }, (o) => { return true; });
            KeyboardSettingViewCommand = new RelayCommand((o) => { CurrentView = KeyboardSettingsVM; }, (o) => { return true; });


        }

    }
}
