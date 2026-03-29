using System.Windows;
using System.Windows.Input;
using Bloxstrap.UI.Elements.About;
using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public class MainWindowViewModel : NotifyPropertyChangedViewModel
    {
        public ICommand OpenAboutCommand => new RelayCommand(OpenAbout);

        public IAsyncRelayCommand SaveSettingsCommand => new AsyncRelayCommand(SaveSettingsAsync);

        public IAsyncRelayCommand SaveAndLaunchSettingsCommand => new AsyncRelayCommand(SaveAndLaunchSettingsAsync);


        public ICommand CloseWindowCommand => new RelayCommand(CloseWindow);

        public EventHandler? RequestSaveNoticeEvent;

        public EventHandler? RequestCloseWindowEvent;

        public bool GBSEnabled = App.GlobalSettings.Loaded;

        public bool TestModeEnabled
        {
            get => App.LaunchSettings.TestModeFlag.Active;
            set
            {
                if (value && !App.State.Prop.TestModeWarningShown)
                {
                    var result = Frontend.ShowMessageBox(Strings.Menu_TestMode_Prompt, MessageBoxImage.Information, MessageBoxButton.YesNo);

                    if (result != MessageBoxResult.Yes)
                        return;

                    App.State.Prop.TestModeWarningShown = true;
                }

                App.LaunchSettings.TestModeFlag.Active = value;
            }
        }

        private void OpenAbout() => new MainWindow().ShowDialog();

        private void CloseWindow() => RequestCloseWindowEvent?.Invoke(this, EventArgs.Empty);

        private async Task SaveSettingsAsync()
        {
            const string LOG_IDENT = "MainWindowViewModel::SaveSettings";

            App.Settings.Save();
            App.State.Save();
            App.FastFlags.Save();
            App.GlobalSettings.Save();

            foreach (var pair in App.PendingSettingTasks)
            {
                var task = pair.Value;

                if (task.Changed)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Executing pending task '{task}'");
                    await Task.Run(task.Execute);
                }
            }

            App.PendingSettingTasks.Clear();

            RequestSaveNoticeEvent?.Invoke(this, EventArgs.Empty);
        }
        public async Task SaveAndLaunchSettingsAsync()
        {
            await SaveSettingsAsync();

            if (!App.LaunchSettings.TestModeFlag.Active) // test mode already launches an instance
                Process.Start(Paths.Application, "-player");
            else
                CloseWindow();
        }
    }
}
