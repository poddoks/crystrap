using System.Windows;
using System.Windows.Input;

using Bloxstrap.UI.ViewModels.Settings;
using Wpf.Ui.Mvvm.Contracts;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for FastFlagsPage.xaml
    /// </summary>
    public partial class GlobalSettingsPage
    {
        private GlobalSettingsViewModel _viewModel = null!;

        public GlobalSettingsPage()
        {
            SetupViewModel();
            InitializeComponent();
        }

        private void SetupViewModel()
        {
            _viewModel = new GlobalSettingsViewModel();

            DataContext = _viewModel;
        }

        private void OpenNvidiaFastFlags_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is INavigationWindow window)
                window.Navigate(typeof(NvidiaFastFlagsPage));
        }

        private void RunRecoveryReset_Click(object sender, RoutedEventArgs e)
        {
            const string LOG_IDENT = "GlobalSettingsPage::RunRecoveryReset";

            var result = Frontend.ShowMessageBox(
                "This will reset Crystrap-managed optimization settings, remove Crystrap-managed Roblox modifications, delete the Crystrap NVIDIA profile, and force a Roblox reinstall on the next launch.\n\nContinue?",
                MessageBoxImage.Warning,
                MessageBoxButton.YesNo,
                MessageBoxResult.No
            );

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                App.Logger.WriteLine(LOG_IDENT, "Starting recovery reset");

                App.Settings.Prop.UseFastFlagManager = false;
                App.Settings.Prop.DisableTerrainTextures = false;
                App.Settings.Prop.ExtremeStripMode = false;
                App.Settings.Prop.EnableBetterMatchmaking = false;
                App.Settings.Prop.EnableBetterMatchmakingRandomization = false;
                App.Settings.Prop.CleanupStaleRobloxFilesBeforeLaunch = false;
                App.Settings.Prop.AutoRepairBrokenRobloxInstallBeforeLaunch = true;

                App.PendingSettingTasks.Clear();
                App.FastFlags.Prop.Clear();
                App.RobloxState.Prop.ModManifest = new();
                App.State.Prop.ForceReinstall = true;

                App.GlobalSettings.SetReadOnly(false);
                App.GlobalSettings.ApplyCrystrapInstallDefaults();
                App.GlobalSettings.SetPreset("Rendering.FramerateCap", 60);
                App.GlobalSettings.SetPreset("Rendering.SavedQualityLevel", 1);

                string nvidiaProfilePath = Path.Combine(Paths.Integrations, "Nvidia", "Crystrap.nip");

                if (File.Exists(nvidiaProfilePath))
                {
                    File.Delete(nvidiaProfilePath);
                    App.Logger.WriteLine(LOG_IDENT, $"Deleted NVIDIA profile at {nvidiaProfilePath}");
                }

                if (Directory.Exists(Paths.Modifications))
                {
                    Directory.Delete(Paths.Modifications, true);
                    App.Logger.WriteLine(LOG_IDENT, $"Deleted modifications directory at {Paths.Modifications}");
                }

                App.Settings.Save();
                App.State.Save();
                App.RobloxState.Save();
                App.GlobalSettings.Save();

                App.Logger.WriteLine(LOG_IDENT, "Recovery reset complete, starting forced Roblox reinstall");

                Process.Start(new ProcessStartInfo
                {
                    FileName = Paths.Application,
                    Arguments = "-player -force",
                    WorkingDirectory = Paths.Base,
                    UseShellExecute = true
                });

                App.Terminate();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Recovery reset failed");
                App.Logger.WriteException(LOG_IDENT, ex);
                Frontend.ShowMessageBox($"Recovery reset failed:\n\n{ex.Message}", MessageBoxImage.Error);
            }
        }

        private void ValidateUInt32(object sender, TextCompositionEventArgs e) => e.Handled = !UInt32.TryParse(e.Text, out uint _);
        private void ValidateFloat(object sender, TextCompositionEventArgs e) => e.Handled = !Regex.IsMatch(e.Text, @"^\d*\.?\d*$");
    }
}
