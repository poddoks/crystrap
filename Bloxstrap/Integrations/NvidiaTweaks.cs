using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace Bloxstrap.Integrations
{
    public static class NvidiaTweaks
    {
        private const string LatestReleaseApiUrl = "https://api.github.com/repos/Orbmu2k/nvidiaProfileInspector/releases/latest";
        private const string ReleaseAssetName = "nvidiaProfileInspector.zip";
        private const string RegistryPath = @"SOFTWARE\Crystrap";
        private const string RegistryValueName = "NvidiaApplied";

        private static readonly string NipXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfProfile>
  <Profile>
    <ProfileName>Roblox VR</ProfileName>
    <Executeables>
      <string>robloxplayerbeta.exe</string>
    </Executeables>
    <Settings>
      <ProfileSetting>
        <SettingNameInfo>Texture filtering - LOD Bias</SettingNameInfo>
        <SettingID>7573135</SettingID>
        <SettingValue>30</SettingValue>
        <ValueType>Dword</ValueType>
      </ProfileSetting>
      <ProfileSetting>
        <SettingNameInfo>Antialiasing - Transparency Supersampling</SettingNameInfo>
        <SettingID>282364549</SettingID>
        <SettingValue>8</SettingValue>
        <ValueType>Dword</ValueType>
      </ProfileSetting>
      <ProfileSetting>
        <SettingNameInfo />
        <SettingID>541081465</SettingID>
        <SettingValue>30</SettingValue>
        <ValueType>Dword</ValueType>
      </ProfileSetting>
    </Settings>
  </Profile>
</ArrayOfProfile>";

        private static readonly string ResetNipXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfProfile>
  <Profile>
    <ProfileName>Roblox VR</ProfileName>
    <Executeables>
      <string>robloxplayerbeta.exe</string>
    </Executeables>
    <Settings>
      <ProfileSetting>
        <SettingNameInfo>Texture filtering - LOD Bias</SettingNameInfo>
        <SettingID>7573135</SettingID>
        <SettingValue>0</SettingValue>
        <ValueType>Dword</ValueType>
      </ProfileSetting>
      <ProfileSetting>
        <SettingNameInfo>Antialiasing - Transparency Supersampling</SettingNameInfo>
        <SettingID>282364549</SettingID>
        <SettingValue>0</SettingValue>
        <ValueType>Dword</ValueType>
      </ProfileSetting>
      <ProfileSetting>
        <SettingNameInfo />
        <SettingID>541081465</SettingID>
        <SettingValue>0</SettingValue>
        <ValueType>Dword</ValueType>
      </ProfileSetting>
    </Settings>
  </Profile>
</ArrayOfProfile>";

        private static string DataDir => Paths.Base;
        private static string NipPath => Path.Combine(DataDir, "Crystrap_NoTextures.nip");
        private static string ResetPath => Path.Combine(DataDir, "Crystrap_Reset.nip");

        public static bool IsNvidiaPresent()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\NVIDIA Corporation");
                return key is not null;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsApplied()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
                return key?.GetValue(RegistryValueName) is int value && value == 1;
            }
            catch
            {
                return false;
            }
        }

        public static string? FindNpi()
        {
            string exeDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName ?? AppContext.BaseDirectory) ?? String.Empty;

            string[] candidates =
            {
                Path.Combine(exeDir, "nvidiaProfileInspector.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "nvidiaProfileInspector.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NVIDIA Inspector", "nvidiaProfileInspector.exe")
            };

            return candidates.FirstOrDefault(File.Exists);
        }

        public static async Task EnsureLatestInstalledAsync()
        {
            const string LOG_IDENT = "NvidiaTweaks::EnsureLatestInstalledAsync";

            if (!IsNvidiaPresent())
            {
                App.Logger.WriteLine(LOG_IDENT, "NVIDIA installation not detected, skipping nvidiaProfileInspector download");
                return;
            }

            using var releaseResponse = await App.HttpClient.GetAsync(LatestReleaseApiUrl);
            releaseResponse.EnsureSuccessStatusCode();

            using var releaseStream = await releaseResponse.Content.ReadAsStreamAsync();
            using var releaseJson = await JsonDocument.ParseAsync(releaseStream);

            string? downloadUrl = null;

            foreach (var asset in releaseJson.RootElement.GetProperty("assets").EnumerateArray())
            {
                if (!String.Equals(asset.GetProperty("name").GetString(), ReleaseAssetName, StringComparison.OrdinalIgnoreCase))
                    continue;

                downloadUrl = asset.GetProperty("browser_download_url").GetString();
                break;
            }

            if (String.IsNullOrEmpty(downloadUrl))
                throw new InvalidOperationException($"Could not find {ReleaseAssetName} in the latest nvidiaProfileInspector release.");

            string tempZip = Path.Combine(DataDir, "nvidiaProfileInspector.latest.zip");
            string tempExtractDirectory = Path.Combine(DataDir, "nvidiaProfileInspector.latest");

            try
            {
                using (var zipResponse = await App.HttpClient.GetAsync(downloadUrl))
                {
                    zipResponse.EnsureSuccessStatusCode();

                    await using var inputStream = await zipResponse.Content.ReadAsStreamAsync();
                    await using var outputStream = File.Create(tempZip);
                    await inputStream.CopyToAsync(outputStream);
                }

                if (Directory.Exists(tempExtractDirectory))
                    Directory.Delete(tempExtractDirectory, true);

                ZipFile.ExtractToDirectory(tempZip, tempExtractDirectory, true);

                foreach (string sourcePath in Directory.GetFiles(tempExtractDirectory))
                {
                    string destinationPath = Path.Combine(DataDir, Path.GetFileName(sourcePath));
                    File.Copy(sourcePath, destinationPath, true);
                }

                App.Logger.WriteLine(LOG_IDENT, "Downloaded latest nvidiaProfileInspector into the Crystrap install directory");
            }
            finally
            {
                if (File.Exists(tempZip))
                    File.Delete(tempZip);

                if (Directory.Exists(tempExtractDirectory))
                    Directory.Delete(tempExtractDirectory, true);
            }
        }

        public static void Apply()
        {
            Directory.CreateDirectory(DataDir);
            File.WriteAllText(NipPath, NipXml, Encoding.Unicode);
            ImportNip(NipPath);
            SetAppliedState(true);
        }

        public static void Reset()
        {
            Directory.CreateDirectory(DataDir);
            File.WriteAllText(ResetPath, ResetNipXml, Encoding.Unicode);
            ImportNip(ResetPath);
            SetAppliedState(false);
        }

        public static string WriteNipOnly(bool reset = false)
        {
            Directory.CreateDirectory(DataDir);

            string path = reset ? ResetPath : NipPath;
            File.WriteAllText(path, reset ? ResetNipXml : NipXml, Encoding.Unicode);

            return path;
        }

        private static void ImportNip(string nipPath)
        {
            string? npiPath = FindNpi();

            if (npiPath is null)
                throw new NpiNotFoundException(nipPath);

            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = npiPath,
                Arguments = $"\"{nipPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            process?.WaitForExit(15000);
        }

        private static void SetAppliedState(bool applied)
        {
            using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
            key?.SetValue(RegistryValueName, applied ? 1 : 0, RegistryValueKind.DWord);
        }
    }

    public class NpiNotFoundException : Exception
    {
        public string NipFilePath { get; }

        public NpiNotFoundException(string nipFilePath) : base("nvidiaProfileInspector.exe not found.")
        {
            NipFilePath = nipFilePath;
        }
    }
}
