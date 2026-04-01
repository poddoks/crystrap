using System.Windows;
using System.Windows.Input;
using Bloxstrap.Integrations;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.ContextMenu
{
    internal class ServerInformationViewModel : NotifyPropertyChangedViewModel
    {
        public string InstanceId => String.Empty;

        public string ServerType => String.Empty;

        public string ServerLocation { get; private set; } = Strings.Common_Loading;

        public string ServerUptime { get; private set; } = Strings.Common_Loading;

        public Visibility ServerLocationVisibility => Visibility.Collapsed;
        public Visibility ServerUptimeVisibility => Visibility.Collapsed;

        public ICommand CopyInstanceIdCommand => new RelayCommand(CopyInstanceId);

        public ServerInformationViewModel(Watcher watcher)
        {
            throw new NotSupportedException("Server information is no longer available.");
        }

        public void QueryServerLocation()
        {
            OnPropertyChanged(nameof(ServerLocation));
        }

        public void QueryServerUptime()
        {
            OnPropertyChanged(nameof(ServerUptime));
        }

        private void CopyInstanceId() => Clipboard.SetDataObject(InstanceId);
    }
}
