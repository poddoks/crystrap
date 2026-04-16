using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Bloxstrap.Models;

namespace Bloxstrap.Integrations
{
    public static class NvidiaProfileManager
    {
        private const string NvidiaInspectorUrl =
            "https://github.com/Orbmu2k/nvidiaProfileInspector/releases/download/2.4.0.34/nvidiaProfileInspector.zip";
        private const string NvidiaProfileName = "Roblox VR";
        private const string NvidiaExecutableName = "robloxplayerbeta.exe";

        private static readonly string InspectorDir = Path.Combine(Paths.Integrations, "Nvidia");
        private static readonly string InspectorExe = Path.Combine(InspectorDir, "nvidiaProfileInspector.exe");
        private static readonly Encoding Utf16Bom = new UnicodeEncoding(false, true);

        public static string EmptyNipTemplate() =>
$@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfProfile>
  <Profile>
    <ProfileName>{NvidiaProfileName}</ProfileName>
    <Executeables>
      <string>{NvidiaExecutableName}</string>
    </Executeables>
    <Settings>
    </Settings>
  </Profile>
</ArrayOfProfile>";

        public static void SaveToNip(string path, IEnumerable<NvidiaEditorEntry> entries)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            var uniqueEntries = entries
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
                .GroupBy(entry => (entry.SettingId, entry.Name))
                .Select(group => group.Last())
                .ToList();

            var settings = new XElement("Settings");

            foreach (var entry in uniqueEntries)
            {
                if (!TryNormalizeSettingId(entry.SettingId, out string fixedId))
                    continue;

                settings.Add(
                    new XElement(
                        "ProfileSetting",
                        new XElement("SettingNameInfo", entry.Name),
                        new XElement("SettingID", fixedId),
                        new XElement("ValueType", NormalizeValueType(entry.ValueType)),
                        new XElement("SettingValue", entry.Value ?? "0")
                    )
                );
            }

            var document = new XDocument(
                new XDeclaration("1.0", "utf-16", null),
                new XElement(
                    "ArrayOfProfile",
                    new XElement(
                        "Profile",
                        new XElement("ProfileName", NvidiaProfileName),
                        new XElement("Executeables", new XElement("string", NvidiaExecutableName)),
                        settings
                    )
                )
            );

            WriteUtf16Xml(path, document);
        }

        public static List<NvidiaEditorEntry> LoadFromNip(string path)
        {
            var results = new List<NvidiaEditorEntry>();

            if (!File.Exists(path))
                return results;

            XDocument document;

            try
            {
                document = XDocument.Load(path);
            }
            catch
            {
                return results;
            }

            foreach (var node in document.Descendants("ProfileSetting"))
            {
                string name = node.Element("SettingNameInfo")?.Value ?? "";
                string id = node.Element("SettingID")?.Value ?? "";
                string value = node.Element("SettingValue")?.Value ?? "0";
                string type = node.Element("ValueType")?.Value ?? "Dword";

                if (!TryNormalizeSettingId(id, out string fixedId))
                    continue;

                results.Add(
                    new NvidiaEditorEntry
                    {
                        Name = string.IsNullOrWhiteSpace(name) ? $"Setting {fixedId}" : name,
                        SettingId = fixedId,
                        Value = value,
                        ValueType = NormalizeValueType(type)
                    }
                );
            }

            return results;
        }

        public static async Task<bool> ApplyNipFile(string nipPath)
        {
            if (!File.Exists(nipPath))
                return false;

            if (!await EnsureInspectorDownloaded())
                return false;

            if (await LaunchImport(nipPath))
                return true;

            await Task.Delay(1000);

            if (await LaunchImport(nipPath))
                return true;

            Frontend.ShowMessageBox(
                "nvidiaProfileInspector was launched, but Crystrap could not confirm the import window in time. If the profile importer opened, complete the import there and apply the changes.",
                System.Windows.MessageBoxImage.Information
            );

            return false;
        }

        private static async Task<bool> EnsureInspectorDownloaded()
        {
            if (File.Exists(InspectorExe))
                return true;

            string tempZipPath = Path.Combine(InspectorDir, "nvidiaProfileInspector.tmp.zip");

            try
            {
                Directory.CreateDirectory(InspectorDir);

                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);

                using (var response = await App.HttpClient.GetAsync(
                    NvidiaInspectorUrl,
                    System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    await using var stream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await response.Content.CopyToAsync(stream);
                    await stream.FlushAsync();
                }

                await WaitForFileUnlock(tempZipPath);
                ZipFile.ExtractToDirectory(tempZipPath, InspectorDir, true);
                File.Delete(tempZipPath);

                return File.Exists(InspectorExe);
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox(
                    "Failed to download NVIDIA Profile Inspector:\n\n" + ex.Message,
                    System.Windows.MessageBoxImage.Error
                );

                if (File.Exists(tempZipPath))
                    File.Delete(tempZipPath);

                return false;
            }
        }

        private static async Task<bool> LaunchImport(string path)
        {
            try
            {
                using var process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = InspectorExe,
                        Arguments = $"\"{path}\"",
                        UseShellExecute = true,
                        Verb = "runas"
                    }
                );

                if (process is null)
                    return false;

                for (int i = 0; i < 50; i++)
                {
                    await Task.Delay(100);
                    process.Refresh();

                    if (process.HasExited)
                        return false;

                    if (process.MainWindowHandle != IntPtr.Zero)
                        return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private static async Task WaitForFileUnlock(string path)
        {
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    using (File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None))
                        return;
                }
                catch (IOException)
                {
                    await Task.Delay(100);
                }
            }
        }

        private static bool TryNormalizeSettingId(string? raw, out string result)
        {
            result = null!;

            if (string.IsNullOrWhiteSpace(raw))
                return false;

            raw = raw.Trim();

            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (!uint.TryParse(raw.AsSpan(2), System.Globalization.NumberStyles.HexNumber, null, out uint hex))
                    return false;

                result = hex.ToString();
                return true;
            }

            if (!uint.TryParse(raw, out uint dec))
                return false;

            result = dec.ToString();
            return true;
        }

        private static string NormalizeValueType(string? valueType)
        {
            return valueType?.ToLowerInvariant() switch
            {
                "string" => "String",
                "binary" => "Binary",
                "boolean" => "Boolean",
                "hex" => "Hex",
                _ => "Dword"
            };
        }

        private static void WriteUtf16Xml(string path, XDocument document)
        {
            using var writer = XmlWriter.Create(
                path,
                new XmlWriterSettings
                {
                    Encoding = Utf16Bom,
                    Indent = true,
                    OmitXmlDeclaration = false
                }
            );

            document.Save(writer);
        }
    }
}
