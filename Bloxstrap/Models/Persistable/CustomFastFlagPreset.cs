namespace Bloxstrap.Models.Persistable
{
    public class CustomFastFlagPreset
    {
        public Dictionary<string, string> FastFlags { get; set; } = new();

        public Dictionary<string, string?> GlobalSettings { get; set; } = new();

        public bool UseFastFlagManager { get; set; } = true;

        public bool EnableBetterMatchmaking { get; set; } = false;

        public bool EnableBetterMatchmakingRandomization { get; set; } = false;

        public bool DisableTerrainTextures { get; set; } = false;

        public bool ExtremeStripMode { get; set; } = false;

        public bool AutoRepairBrokenRobloxInstallBeforeLaunch { get; set; } = true;

        public bool CleanupStaleRobloxFilesBeforeLaunch { get; set; } = false;
    }
}
