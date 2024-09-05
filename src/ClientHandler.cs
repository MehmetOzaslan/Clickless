using Clickless.MVVM.Models;

namespace Clickless
{
    public static class ClientHandler
    {
        static SelectedClientSettingsModel selectedClientSettings = new SelectedClientSettingsModel();
        static IRectEngine _rectEngine;

        private static void UpdateEngine()
        {
            switch (selectedClientSettings.selectedClient)
            {
                case Client.GRID:
                    _rectEngine = new GridClient();
                    break;
                case Client.ML:
                    _rectEngine = MLClient.Instance;
                    break;
                default:
                    break;
            }
        }

        public static IRectEngine GetEngine()
        {
            if(_rectEngine == null)
            {
                UpdateEngine();
            }
            return _rectEngine;
        }

        public static void UpdateSettings(SelectedClientSettingsModel s)
        {
            if (s.selectedClient != selectedClientSettings.selectedClient) {
                selectedClientSettings = s;
            }
            UpdateEngine();
        }
    }
}
