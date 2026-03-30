using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;

using Bloxstrap.Enums;
using Bloxstrap.UI.Elements.Bootstrapper.Base;
using Microsoft.Win32;

namespace Bloxstrap
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#if QA_BUILD
        public const string ProjectName = "Crystrap-QA";
#else
        public const string ProjectName = "Crystrap";
#endif
        public const string ProjectOwner = "poddoks";
        public const string ProjectRepository = "poddoks/crystrap";
        public const string ProjectDownloadLink = "https://github.com/poddoks/crystrap";
        public const string ProjectHelpLink = "https://github.com/poddoks/crystrap";
        public const string ProjectSupportLink = "https://github.com/poddoks/crystrap";
        public const string ProjectRemoteDataLink = "https://config.crystrap.app/v1/Data.json";
        public const string ProjectReleaseAssetName = "Crystrap.exe";

        public const string RobloxPlayerAppName = "RobloxPlayerBeta.exe";
        public const string RobloxStudioAppName = "RobloxStudioBeta.exe";

        // simple shorthand for extremely frequently used and long string - this goes under HKCU
        public const string UninstallKey = $@"Software\Microsoft\Windows\CurrentVersion\Uninstall\{ProjectName}";

        public const string ApisKey = $"Software\\{ProjectName}";

        public static LaunchSettings LaunchSettings { get; private set; } = null!;

        public static BuildMetadataAttribute BuildMetadata = Assembly.GetExecutingAssembly().GetCustomAttribute<BuildMetadataAttribute>()!;

        public static string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString()[..^2];

        public static Bootstrapper? Bootstrapper { get; set; } = null!;

        public static bool IsActionBuild => !String.IsNullOrEmpty(BuildMetadata.CommitRef);

        public static bool IsProductionBuild => IsActionBuild && BuildMetadata.CommitRef.StartsWith("tag", StringComparison.Ordinal);

        public static bool IsStudioVisible => !String.IsNullOrEmpty(App.RobloxState.Prop.Studio.VersionGuid);

        public static readonly MD5 MD5Provider = MD5.Create();

        public static readonly Logger Logger = new();

        public static readonly Dictionary<string, BaseTask> PendingSettingTasks = new();

        public static readonly JsonManager<Settings> Settings = new();

        public static readonly JsonManager<State> State = new();

        public static readonly JsonManager<RobloxState> RobloxState = new();

        public static readonly RemoteDataManager RemoteData = new();

        public static readonly FastFlagManager FastFlags = new();

        public static readonly GlobalSettingsManager GlobalSettings = new();

        public static readonly CookiesManager Cookies = new();

        public static readonly HttpClient HttpClient = new(
            new HttpClientLoggingHandler(
                new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All }
            )
        );

        private static bool _showingExceptionDialog = false;

        public static void Terminate(ErrorCode exitCode = ErrorCode.ERROR_SUCCESS)
        {
            int exitCodeNum = (int)exitCode;

            Logger.WriteLine("App::Terminate", $"Terminating with exit code {exitCodeNum} ({exitCode})");

            Environment.Exit(exitCodeNum);
        }

        public static void SoftTerminate(ErrorCode exitCode = ErrorCode.ERROR_SUCCESS)
        {
            int exitCodeNum = (int)exitCode;

            Logger.WriteLine("App::SoftTerminate", $"Terminating with exit code {exitCodeNum} ({exitCode})");

            Current.Dispatcher.Invoke(() => Current.Shutdown(exitCodeNum));
        }

        void GlobalExceptionHandler(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            Logger.WriteLine("App::GlobalExceptionHandler", "An exception occurred");

            FinalizeExceptionHandling(e.Exception);
        }

        public static void FinalizeExceptionHandling(AggregateException ex)
        {
            foreach (var innerEx in ex.InnerExceptions)
                Logger.WriteException("App::FinalizeExceptionHandling", innerEx);

            FinalizeExceptionHandling(ex.GetBaseException(), false);
        }

        public static void FinalizeExceptionHandling(Exception ex, bool log = true)
        {
            if (log)
                Logger.WriteException("App::FinalizeExceptionHandling", ex);

            if (_showingExceptionDialog)
                return;

            _showingExceptionDialog = true;

            SendLog();

            if (Bootstrapper?.Dialog != null)
            {
                if (Bootstrapper.Dialog.TaskbarProgressValue == 0)
                    Bootstrapper.Dialog.TaskbarProgressValue = 1; // make sure it's visible

                Bootstrapper.Dialog.TaskbarProgressState = TaskbarItemProgressState.Error;
            }

            Frontend.ShowExceptionDialog(ex);

            Terminate(ErrorCode.ERROR_INSTALL_FAILURE);
        }

        public static async Task<GithubRelease?> GetLatestRelease()
        {
            const string LOG_IDENT = "App::GetLatestRelease";

            try
            {
                var releaseInfo = await Http.GetJson<GithubRelease>($"https://api.github.com/repos/{ProjectRepository}/releases/latest");

                if (releaseInfo is null || releaseInfo.Assets is null)
                {
                    Logger.WriteLine(LOG_IDENT, "Encountered invalid data");
                    return null;
                }

                return releaseInfo;
            }
            catch (Exception ex)
            {
                Logger.WriteException(LOG_IDENT, ex);
            }

            return null;
        }

        public static GithubReleaseAsset? GetLatestReleaseAsset(GithubRelease releaseInfo)
        {
            var assets = releaseInfo.Assets;

            if (assets is null || assets.Count == 0)
                return null;

            return assets.FirstOrDefault(x => String.Equals(x.Name, ProjectReleaseAssetName, StringComparison.OrdinalIgnoreCase))
                ?? assets.FirstOrDefault(x => x.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                ?? assets.FirstOrDefault();
        }

        public static async Task<bool> CheckForUpdatesAsync(LaunchMode launchMode = LaunchMode.None, IBootstrapperDialog? dialog = null)
        {
            const string LOG_IDENT = "App::CheckForUpdatesAsync";

            if (LaunchSettings.BypassUpdateCheck || LaunchSettings.UpgradeFlag.Active || !Settings.Prop.CheckForUpdates)
                return false;

            // don't update if there's another instance running (likely running in the background)
            if (Process.GetProcessesByName(ProjectName).Length > 1)
            {
                Logger.WriteLine(LOG_IDENT, $"More than one {ProjectName} instance running, aborting update check");
                return false;
            }

            Logger.WriteLine(LOG_IDENT, "Checking for updates...");

            var releaseInfo = await GetLatestRelease();

            if (releaseInfo is null)
                return false;

            var versionComparison = Utilities.CompareVersions(Version, releaseInfo.TagName);

            if (versionComparison == VersionComparison.Equal || versionComparison == VersionComparison.GreaterThan)
            {
                Logger.WriteLine(LOG_IDENT, "No updates found");
                return false;
            }

            if (dialog is not null)
                dialog.CancelEnabled = false;

            string version = releaseInfo.TagName;

            try
            {
                var asset = GetLatestReleaseAsset(releaseInfo);

                if (asset is null)
                    throw new InvalidOperationException("Latest release does not contain a downloadable asset.");

                string downloadLocation = Path.Combine(Paths.TempUpdates, asset.Name);

                Directory.CreateDirectory(Paths.TempUpdates);

                Logger.WriteLine(LOG_IDENT, $"Downloading {releaseInfo.TagName}...");

                if (File.Exists(downloadLocation))
                {
                    try
                    {
                        string? existingDownloadedVersion = FileVersionInfo.GetVersionInfo(downloadLocation).ProductVersion;

                        if (!String.Equals(existingDownloadedVersion, releaseInfo.TagName, StringComparison.OrdinalIgnoreCase))
                        {
                            Logger.WriteLine(LOG_IDENT, $"Removing stale updater payload {downloadLocation} (found {existingDownloadedVersion ?? "unknown"}, expected {releaseInfo.TagName})");
                            File.Delete(downloadLocation);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteLine(LOG_IDENT, $"Failed to inspect existing updater payload at {downloadLocation}, forcing refresh");
                        Logger.WriteException(LOG_IDENT, ex);

                        try
                        {
                            File.Delete(downloadLocation);
                        }
                        catch (Exception deleteEx)
                        {
                            Logger.WriteLine(LOG_IDENT, $"Failed to delete stale updater payload at {downloadLocation}");
                            Logger.WriteException(LOG_IDENT, deleteEx);
                        }
                    }
                }

                if (!File.Exists(downloadLocation))
                {
                    var response = await HttpClient.GetAsync(asset.BrowserDownloadUrl);

                    await using var fileStream = new FileStream(downloadLocation, FileMode.Create, FileAccess.Write);
                    await response.Content.CopyToAsync(fileStream);
                }

                Logger.WriteLine(LOG_IDENT, $"Starting {version}...");

                ProcessStartInfo startInfo = new()
                {
                    FileName = downloadLocation,
                };

                startInfo.ArgumentList.Add("-upgrade");

                bool forwardedRobloxProtocolArg = false;

                foreach (string arg in LaunchSettings.Args)
                {
                    if (!forwardedRobloxProtocolArg
                        && (arg.StartsWith("roblox:", StringComparison.OrdinalIgnoreCase)
                            || arg.StartsWith("roblox-player:", StringComparison.OrdinalIgnoreCase)))
                    {
                        startInfo.ArgumentList.Add("-player");
                        startInfo.ArgumentList.Add(arg);
                        forwardedRobloxProtocolArg = true;
                        continue;
                    }

                    if (!forwardedRobloxProtocolArg
                        && (arg.StartsWith("roblox-studio:", StringComparison.OrdinalIgnoreCase)
                            || arg.StartsWith("roblox-studio-auth:", StringComparison.OrdinalIgnoreCase)))
                    {
                        startInfo.ArgumentList.Add("-studio");
                        startInfo.ArgumentList.Add(arg);
                        forwardedRobloxProtocolArg = true;
                        continue;
                    }

                    startInfo.ArgumentList.Add(arg);
                }

                if (launchMode == LaunchMode.Player && !startInfo.ArgumentList.Contains("-player"))
                    startInfo.ArgumentList.Add("-player");
                else if (launchMode == LaunchMode.Studio && !startInfo.ArgumentList.Contains("-studio"))
                    startInfo.ArgumentList.Add("-studio");

                Settings.Save();

                new InterProcessLock("AutoUpdater");

                Process.Start(startInfo);

                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLine(LOG_IDENT, "An exception occurred when running the auto-updater");
                Logger.WriteException(LOG_IDENT, ex);

                Frontend.ShowMessageBox(
                    string.Format(Strings.Bootstrapper_AutoUpdateFailed, version),
                    MessageBoxImage.Information
                );

                Utilities.ShellExecute(ProjectDownloadLink);
            }

            return false;
        }

        public static void SendLog()
        {

        }

        public static void AssertWindowsOSVersion()
        {
            const string LOG_IDENT = "App::AssertWindowsOSVersion";

            int major = Environment.OSVersion.Version.Major;
            if (major < 10) // Windows 10 and newer only
            {
                Logger.WriteLine(LOG_IDENT, $"Detected unsupported Windows version ({Environment.OSVersion.Version}).");

                if (!LaunchSettings.QuietFlag.Active)
                    Frontend.ShowMessageBox(Strings.App_OSDeprecation_Win7_81, MessageBoxImage.Error);

                Terminate(ErrorCode.ERROR_INVALID_FUNCTION);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            const string LOG_IDENT = "App::OnStartup";

            Locale.Initialize();

            base.OnStartup(e);

            Logger.WriteLine(LOG_IDENT, $"Starting {ProjectName} v{Version}");

            string userAgent = $"{ProjectName}/{Version}";

            if (IsActionBuild)
            {
                Logger.WriteLine(LOG_IDENT, $"Compiled {BuildMetadata.Timestamp.ToFriendlyString()} from commit {BuildMetadata.CommitHash} ({BuildMetadata.CommitRef})");

                if (IsProductionBuild)
                    userAgent += $" (Production)";
                else
                    userAgent += $" (Artifact {BuildMetadata.CommitHash}, {BuildMetadata.CommitRef})";
            }
            else
            {
                Logger.WriteLine(LOG_IDENT, $"Compiled {BuildMetadata.Timestamp.ToFriendlyString()} from {BuildMetadata.Machine}");

#if QA_BUILD
                userAgent += " (QA)";
#else
                userAgent += $" (Build {Convert.ToBase64String(Encoding.UTF8.GetBytes(BuildMetadata.Machine))})";
#endif
            }

            Logger.WriteLine(LOG_IDENT, $"OSVersion: {Environment.OSVersion}");
            Logger.WriteLine(LOG_IDENT, $"Loaded from {Paths.Process}");
            Logger.WriteLine(LOG_IDENT, $"Temp path is {Paths.Temp}");
            Logger.WriteLine(LOG_IDENT, $"WindowsStartMenu path is {Paths.WindowsStartMenu}");

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            HttpClient.Timeout = TimeSpan.FromSeconds(30);
            HttpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

            LaunchSettings = new LaunchSettings(e.Args);

            // installation check begins here
            using var uninstallKey = Registry.CurrentUser.OpenSubKey(UninstallKey);
            string? installLocation = null;
            bool fixInstallLocation = false;

            if (uninstallKey?.GetValue("InstallLocation") is string value)
            {
                if (Directory.Exists(value))
                {
                    installLocation = value;
                }
                else
                {
                    // check if user profile folder has been renamed
                    var match = Regex.Match(value, @"^[a-zA-Z]:\\Users\\([^\\]+)", RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        string newLocation = value.Replace(match.Value, Paths.UserProfile, StringComparison.InvariantCultureIgnoreCase);

                        if (Directory.Exists(newLocation))
                        {
                            installLocation = newLocation;
                            fixInstallLocation = true;
                        }
                    }
                }
            }

            // silently change install location if we detect a portable run
            if (installLocation is null && Directory.GetParent(Paths.Process)?.FullName is string processDir)
            {
                var files = Directory.GetFiles(processDir).Select(x => Path.GetFileName(x)).ToArray();

                // check if settings.json and state.json are the only files in the folder
                if (files.Length <= 3 && files.Contains("Settings.json") && files.Contains("State.json"))
                {
                    installLocation = processDir;
                    fixInstallLocation = true;
                }
            }

            if (fixInstallLocation && installLocation is not null)
            {
                var installer = new Installer
                {
                    InstallLocation = installLocation,
                    IsImplicitInstall = true
                };

                if (installer.CheckInstallLocation())
                {
                    Logger.WriteLine(LOG_IDENT, $"Changing install location to '{installLocation}'");
                    installer.DoInstall();
                }
                else
                {
                    // force reinstall
                    installLocation = null;
                }
            }

            if (installLocation is null)
            {
                Logger.Initialize(true);
                AssertWindowsOSVersion();
                Logger.WriteLine(LOG_IDENT, "Not installed, launching the installer");
                AssertWindowsOSVersion(); // prevent new installs from unsupported operating systems
                LaunchHandler.LaunchInstaller();
            }
            else
            {
                Paths.Initialize(installLocation);

                Logger.Initialize(LaunchSettings.UninstallFlag.Active);

                if (!Logger.Initialized && !Logger.NoWriteMode)
                {
                    Logger.WriteLine(LOG_IDENT, "Possible duplicate launch detected, terminating.");
                    Terminate();
                }

                Settings.Load();
                State.Load();
                RobloxState.Load();
                FastFlags.Load();
                GlobalSettings.Load();

                if (Settings.Prop.AllowCookieAccess)
                    Task.Run(Cookies.LoadCookies);

                if (!Locale.SupportedLocales.ContainsKey(Settings.Prop.Locale))
                {
                    Settings.Prop.Locale = "nil";
                    Settings.Save();
                }

                Locale.Set(Settings.Prop.Locale);

                if (!LaunchSettings.BypassUpdateCheck)
                    Installer.HandleUpgrade();

                Task.Run(App.RemoteData.LoadData); // ok

                WindowsRegistry.RegisterApis(); // we want to register those early on
                                                // so we wont have any issues with bloxshade

                string startMenuShortcut = Path.Combine(Paths.WindowsStartMenu, $"{ProjectName}.lnk");
                if (File.Exists(startMenuShortcut))
                    Installer.RefreshInstalledShortcuts();

                LaunchHandler.ProcessLaunchArgs();
            }

            // you must *explicitly* call terminate when everything is done, it won't be called implicitly
        }
    }
}
