using Clickless.Core;
using System.Windows;

namespace Clickless.MVVM.ViewModel
{
    class MainWindowViewModel :	ObservableObject
    {
		public DetectionSettingsViewModel DetectionSettingsVM { get; set; }
        public KeyboardSettingsViewModel KeyboardSettingsVM{ get; set; }
        public HelpViewModel HelpVM { get; set; }
        public GridSettingsViewModel GridSettingsVM { get; set; }
        public SelectedClientSettingsViewModel SelectedClientSettingsVM { get; set; }
        public RelayCommand HelpVMViewCommand {  get; set; }
        public RelayCommand DetectionSettingViewCommand { get; set; }
		public RelayCommand KeyboardSettingViewCommand { get; set; }
        public RelayCommand GridSettingsViewCommand {  get; set; }
        public RelayCommand SelectedClientViewCommand {  get; set; }
        public RelayCommand RestoreWindowCommand { get; set; }
        public RelayCommand MinimizeWindowCommand { get; set; }
        public RelayCommand MaximizeWindowCommand { get; set; }
        public WindowState WindowState { get; set; }
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
            //The models nested in the main window.
            DetectionSettingsVM = new DetectionSettingsViewModel();
            KeyboardSettingsVM = new KeyboardSettingsViewModel();
            HelpVM = new HelpViewModel();  
            GridSettingsVM = new GridSettingsViewModel();
            SelectedClientSettingsVM = new SelectedClientSettingsViewModel();

            //Registering commands for use in the buttons.
            GridSettingsViewCommand = new RelayCommand((o) => { CurrentView = GridSettingsVM; });
            SelectedClientViewCommand = new RelayCommand((o) => { CurrentView = SelectedClientSettingsVM; });
			DetectionSettingViewCommand = new RelayCommand( (o) => { CurrentView = DetectionSettingsVM; });
            KeyboardSettingViewCommand = new RelayCommand((o) => { CurrentView = KeyboardSettingsVM;  });
            HelpVMViewCommand = new RelayCommand((o) => { CurrentView = HelpVM; });

            //Minimize, maximize, and restore commands for manually setting in the window.
            RestoreWindowCommand = new RelayCommand((o) => { RestoreWindow(); });
            MinimizeWindowCommand = new RelayCommand((o) => { MinimizeWindow(); });
            MaximizeWindowCommand = new RelayCommand((o) => { MaximizeWindow(); });

            //Initializing the currentview.
            CurrentView = DetectionSettingsVM;
        }
        
        public void MinimizeWindow(){
            WindowState = WindowState.Minimized;
        }
        public void MaximizeWindow(){
            WindowState = WindowState.Maximized;
        }
        public void RestoreWindow(){
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
        }

    }
}
