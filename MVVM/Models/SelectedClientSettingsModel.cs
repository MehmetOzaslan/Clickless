using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clickless.MVVM.Models
{

    public enum Client
    {
        GRID = 1,
        ML = 2
    }


    public class SelectedClientSettingsModel : IFileNameProvider
    {
        public static readonly string settingsFile = "selectedClient.json";
        public Client selectedClient {  get; set; }

        public SelectedClientSettingsModel() {
            selectedClient = Client.GRID;
        }

        public string GetFileName() { return settingsFile; }
    }
}
