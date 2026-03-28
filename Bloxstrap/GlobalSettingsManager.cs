using Bloxstrap.Enums.FlagPresets;
using Bloxstrap.Enums.GBSPresets;
using Microsoft.VisualBasic;
using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Bloxstrap
{
    public class GlobalSettingsManager
    {
        public XDocument? Document { get; set; } = null!;

        public Dictionary<string, string> PresetPaths = new()
        {
            { "Rendering.FramerateCap", "{UserSettings}/int[@name='FramerateCap']" },
            { "Rendering.SavedQualityLevel", "{UserSettings}/token[@name='SavedQualityLevel']" }, // 0 is automatic
            
            { "User.MouseSensitivity", "{UserSettings}/float[@name='MouseSensitivity']"},
            { "User.VREnabled", "{UserSettings}/bool[@name='VREnabled']"},

            // mostly accessibility
            { "UI.Transparency", "{UserSettings}/float[@name='PreferredTransparency']" },
            { "UI.ReducedMotion", "{UserSettings}/bool[@name='ReducedMotion']" },
            { "UI.FontSize", "{UserSettings}/token[@name='PreferredTextSize']" }
        };

        // we are making it easier for ourselves
        // basically replacing {...} with a path
        // might expand in the future (studio support)
        public Dictionary<string, string> RootPaths = new()
        {
            { "UserSettings", "//Item[@class='UserGameSettings']/Properties" },
        };

        public static IReadOnlyDictionary<FontSize, string?> FontSizes => new Dictionary<FontSize, string?>
        {
            { FontSize.x1, "1" },
            { FontSize.x2, "2" },
            { FontSize.x3, "3" },
            { FontSize.x4, "4" }
        };

        public bool Loaded { get; set; } = false;

        public string FileLocation => Path.Combine(Paths.Roblox, "GlobalBasicSettings_13.xml");

        private static XDocument CreateDefaultDocument()
        {
            return new XDocument(
                new XElement("roblox",
                    new XAttribute("version", "4"),
                    new XElement("Item",
                        new XAttribute("class", "UserGameSettings"),
                        new XElement("Properties",
                            new XElement("int", new XAttribute("name", "FramerateCap"), "60"),
                            new XElement("token", new XAttribute("name", "SavedQualityLevel"), "1"),
                            new XElement("float", new XAttribute("name", "MouseSensitivity"), "0.200000003"),
                            new XElement("bool", new XAttribute("name", "VREnabled"), "False"),
                            new XElement("float", new XAttribute("name", "PreferredTransparency"), "1"),
                            new XElement("bool", new XAttribute("name", "ReducedMotion"), "False"),
                            new XElement("token", new XAttribute("name", "PreferredTextSize"), "1")
                        )
                    )
                )
            );
        }

        public void SetPreset(string prefix, object? value)
        {
            foreach (var pair in PresetPaths.Where(x => x.Key.StartsWith(prefix)))
                SetValue(pair.Value, value);
        }

        public string? GetPreset(string prefix)
        {
            if (!PresetPaths.ContainsKey(prefix))
                return null;

            return GetValue(PresetPaths[prefix]);
        }

        public void SetValue(string path, object? value)
        {
            path = ResolvePath(path);

            XElement? element = Document?.XPathSelectElement(path);
            if (element is null)
                return;

            element.Value = value?.ToString()!;
        }

        public string? GetValue(string path)
        {
            path = ResolvePath(path);

            return Document?.XPathSelectElement(path)?.Value;
        }

        public bool previousReadOnlyState;

        public void SetReadOnly(bool readOnly, bool preserveState = false)
        {
            const string LOG_IDENT = "GBSEditor::SetReadOnly";

            if (!File.Exists(FileLocation))
                return;

            try
            {
                FileAttributes attributes = File.GetAttributes(FileLocation);

                if (readOnly)
                    attributes |= FileAttributes.ReadOnly;
                else
                    attributes &= ~FileAttributes.ReadOnly;

                File.SetAttributes(FileLocation, attributes);

                if (!preserveState)
                    previousReadOnlyState = readOnly;
            } catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, $"Failed to set read-only on {FileLocation}");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public bool GetReadOnly()
        {
            if (!File.Exists(FileLocation))
                return false;

            return File.GetAttributes(FileLocation).HasFlag(FileAttributes.ReadOnly);
        }

        public void Load()
        {
            string LOG_IDENT = "GBSEditor::Load";

            App.Logger.WriteLine(LOG_IDENT, $"Loading from {FileLocation}...");

            if (!File.Exists(FileLocation))
            {
                try
                {
                    Directory.CreateDirectory(Paths.Roblox);
                    Document = CreateDefaultDocument();
                    Document.Save(FileLocation);
                    SetReadOnly(false);
                    previousReadOnlyState = false;
                }
                catch (Exception ex)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Failed to create default global settings file!");
                    App.Logger.WriteException(LOG_IDENT, ex);
                    return;
                }
            }

            try
            {
                Document = XDocument.Load(FileLocation);
                Loaded = true;

                previousReadOnlyState = GetReadOnly();
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to load!");
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }

        public virtual void Save()
        {
            string LOG_IDENT = "GBSEditor::Save";

            App.Logger.WriteLine(LOG_IDENT, $"Saving to {FileLocation}...");

            try
            {
                SetReadOnly(false, true);
                Document?.Save(FileLocation);

                SetReadOnly(previousReadOnlyState);
            }
            catch (Exception ex)
            {
                App.Logger.WriteLine(LOG_IDENT, "Failed to save");
                App.Logger.WriteException(LOG_IDENT, ex);

                return;
            }

            App.Logger.WriteLine(LOG_IDENT, "Save complete!");
        }

        private string ResolvePath(string rawPath)
        {
            return Regex.Replace(rawPath, @"\{(.+?)\}", match =>
            {
                string key = match.Groups[1].Value;
                return RootPaths.TryGetValue(key, out var value) ? value : match.Value; ;
            });
        }
    }
}
