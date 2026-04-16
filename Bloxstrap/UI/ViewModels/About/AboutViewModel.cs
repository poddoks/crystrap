using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Windows;
using System.Windows.Input;

using CommunityToolkit.Mvvm.Input;

namespace Bloxstrap.UI.ViewModels.About
{
    public class AboutViewModel : NotifyPropertyChangedViewModel
    {
        private static readonly X509Certificate2? SigningCertificate = GetSigningCertificate();

        public string Version => string.Format(Strings.Menu_About_Version, App.Version);

        public BuildMetadataAttribute BuildMetadata => App.BuildMetadata;

        public string BuildTimestamp => BuildMetadata.Timestamp.ToFriendlyString();
        public string BuildCommitHashUrl => $"https://github.com/{App.ProjectRepository}/commit/{BuildMetadata.CommitHash}";
        public string ProjectRepositoryUrl => $"https://github.com/{App.ProjectRepository}";
        public string ProjectReleasesUrl => $"{ProjectRepositoryUrl}/releases";
        public string ProjectLicenseUrl => $"{ProjectRepositoryUrl}/blob/main/LICENSE";
        public string LicenseName => "MIT License";
        public string LicenseSummary => "Crystrap is open-source software distributed under the MIT License.";
        public string SignatureStatus => SigningCertificate is null ? "Unsigned build" : "Signed build";
        public string SignaturePublisher => SigningCertificate?.GetNameInfo(X509NameType.SimpleName, false) ?? "No Authenticode signature detected for this executable.";

        public Visibility BuildInformationVisibility => App.IsProductionBuild ? Visibility.Collapsed : Visibility.Visible;
        public Visibility BuildCommitVisibility => App.IsActionBuild ? Visibility.Visible : Visibility.Collapsed;

        public ICommand LaunchUninstallerCommand => new RelayCommand(LaunchUninstaller);

        private void LaunchUninstaller()
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Paths.Process,
                Arguments = "-uninstall",
                UseShellExecute = true
            });

            App.SoftTerminate();
        }

        private static X509Certificate2? GetSigningCertificate()
        {
            try
            {
                return new X509Certificate2(X509Certificate.CreateFromSignedFile(Paths.Process));
            }
            catch
            {
                return null;
            }
        }
    }
}
