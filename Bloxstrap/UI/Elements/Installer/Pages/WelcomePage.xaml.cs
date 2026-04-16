using System.Windows;
using System.Windows.Navigation;

using Bloxstrap.UI.ViewModels.Installer;

namespace Bloxstrap.UI.Elements.Installer.Pages
{
    /// <summary>
    /// Interaction logic for WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage
    {
        private readonly WelcomeViewModel _viewModel = new();

        public WelcomePage()
        {
            if (Window.GetWindow(this) is MainWindow window)
                window.SetButtonEnabled("next", true);

            DataContext = _viewModel;
            InitializeComponent();
        }

        private void UiPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow window)
                window.SetNextButtonText(Strings.Common_Navigation_Next);
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void ReleasesButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/poddoks/crystrap/releases") { UseShellExecute = true });
        }

        private void IssuesButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/poddoks/crystrap/issues") { UseShellExecute = true });
        }
    }
}
