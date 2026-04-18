using System.Collections.ObjectModel;

namespace Bloxstrap.Models.Persistable
{
    public class Settings
    {
        // uh
        public bool AllowCookieAccess { get; set; } = false;

        // bloxstrap configuration
        public BootstrapperStyle BootstrapperStyle { get; set; } = BootstrapperStyle.FluentAeroDialog;
        public BootstrapperIcon BootstrapperIcon { get; set; } = BootstrapperIcon.IconCrystrap;
        public string BootstrapperTitle { get; set; } = App.ProjectName;
        public string BootstrapperIconCustomLocation { get; set; } = "";
        public RobloxIcon RobloxIcon { get; set; } = RobloxIcon.IconDefault;
        public string RobloxTitle { get; set; } = "Roblox";
        public string RobloxIconCustomLocation { get; set; } = "";
        public Theme Theme { get; set; } = Theme.Default;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool DeveloperMode { get; set; } = false;
        public bool CheckForUpdates { get; set; } = true;
        public bool MultiInstanceLaunching { get; set; } = false;
        public bool ConfirmLaunches { get; set; } = true;
        public string Locale { get; set; } = "nil";
        public bool ForceRobloxLanguage { get; set; } = false;
        public bool UseFastFlagManager { get; set; } = true;
        public bool WPFSoftwareRender { get; set; } = false;
        public bool EnableAnalytics { get; set; } = false;
        public bool UpdateRoblox { get; set; } = true;
        public bool StaticDirectory { get; set; } = false;
        public string Channel { get; set; } = RobloxInterfaces.Deployment.DefaultChannel;
        public ChannelChangeMode ChannelChangeMode { get; set; } = ChannelChangeMode.Automatic;
        public string ChannelHash { get; set; } = "";
        public string DownloadingStringFormat { get; set; } = Strings.Bootstrapper_Status_Downloading + " {0} - {1}MB / {2}MB";
        public string? SelectedCustomTheme { get; set; } = null;
        public string LastSelectedFastFlagPreset { get; set; } = "";
        public Dictionary<string, CustomFastFlagPreset> CustomFastFlagPresets { get; set; } = new();
        public bool BackgroundUpdatesEnabled { get; set; } = false;
        public bool DebugDisableVersionPackageCleanup { get; set; } = false;
        public bool EnableBetterMatchmaking { get; set; } = false;
        public bool EnableBetterMatchmakingRandomization { get; set; } = false;
        public bool DisableTerrainTextures { get; set; } = false;
        public bool ExtremeStripMode { get; set; } = false;
        public bool AutoRepairBrokenRobloxInstallBeforeLaunch { get; set; } = true;
        public bool CleanupStaleRobloxFilesBeforeLaunch { get; set; } = false;
        public WebEnvironment WebEnvironment { get; set; } = WebEnvironment.Production;

        // integration configuration
        public CleanerOptions CleanerOptions { get; set; } = CleanerOptions.Never;
        public List<string> CleanerDirectories { get; set; } = new List<string>();
        public ObservableCollection<CustomIntegration> CustomIntegrations { get; set; } = new();
    }
}
