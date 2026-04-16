using System.Windows;

using Bloxstrap.UI.ViewModels.Settings;

using Wpf.Ui.Mvvm.Contracts;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    public partial class NvidiaFastFlagsPage
    {
        private readonly NvidiaFastFlagsViewModel _viewModel;

        public NvidiaFastFlagsPage()
        {
            InitializeComponent();
            _viewModel = new NvidiaFastFlagsViewModel();
            DataContext = _viewModel;
        }

        private void OpenRawEditor_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is INavigationWindow window)
                window.Navigate(typeof(NvidiaFFlagEditorPage));
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.Apply();
        }

        private void OpenFastFlagSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is INavigationWindow window)
                window.Navigate(typeof(FastFlagsPage));
        }
    }
}
