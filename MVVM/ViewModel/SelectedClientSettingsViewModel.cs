using Clickless.Core;
using Clickless.MVVM.Models;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.MVVM.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    class SelectedClientSettingsViewModel : ObservableObject
    {
        public SelectedClientSettingsModel clientSettingsModel;

        private SelectedClientSettingsModel LoadSettings()
        {
            return ObjectSerializer.LoadDataOrDefault<SelectedClientSettingsModel>();
        }

        private void SaveSettings()
        {
            ObjectSerializer.SaveData(clientSettingsModel);
        }

        private Client _selectedClient;
        public Client SelectedClient
        {
            get => _selectedClient;
            set
            {
                if (_selectedClient != value)
                {
                    _selectedClient = value;
                    clientSettingsModel.selectedClient = value;
                    UpdateClientFromChosenSetting();
                    SaveSettings();
                    OnPropertyChanged();
                }
            }
        }

        public void UpdateClientFromChosenSetting()
        {
            ClientHandler.UpdateSettings(clientSettingsModel);
        }

        public SelectedClientSettingsViewModel()
        {
            clientSettingsModel = LoadSettings();
            _selectedClient = clientSettingsModel.selectedClient;
            UpdateClientFromChosenSetting();
        }
    }
}
