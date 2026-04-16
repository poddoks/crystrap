using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

using Microsoft.Win32;

using Bloxstrap.Models;

namespace Bloxstrap.UI.Elements.Dialogs
{
    public partial class AddNvidiaFFlagWindow
    {
        public List<NvidiaEditorEntry> ResultEntries { get; } = new();

        public AddNvidiaFFlagWindow()
        {
            InitializeComponent();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ResultEntries.Clear();

            bool success = Tabs.SelectedIndex switch
            {
                0 => TryAddSingle(),
                2 => TryAddFullValue(),
                _ => TryImportNip()
            };

            if (!success)
                return;

            DialogResult = true;
            Close();
        }

        private bool TryAddSingle()
        {
            string name = NameBox.Text.Trim();
            string settingId = SettingIdBox.Text.Trim();
            string value = ValueBox.Text.Trim();
            string valueType = ((ComboBoxItem)ValueTypeBox.SelectedItem).Content.ToString()!;

            if (string.IsNullOrWhiteSpace(name))
            {
                Frontend.ShowMessageBox("Setting name is required.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(settingId))
            {
                Frontend.ShowMessageBox("Setting ID is required.");
                return false;
            }

            ResultEntries.Add(
                new NvidiaEditorEntry
                {
                    Name = name,
                    SettingId = settingId,
                    Value = string.IsNullOrWhiteSpace(value) ? "0" : value,
                    ValueType = NormalizeValueType(valueType)
                }
            );

            return true;
        }

        private bool TryAddFullValue()
        {
            string fullValue = FullValueBox.Text.Trim();
            string[] parts = fullValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 4)
            {
                Frontend.ShowMessageBox("Please enter the full NVIDIA setting data.");
                return false;
            }

            string name = string.Join(" ", parts.Take(parts.Length - 3));
            string settingId = parts[^3];
            string value = parts[^2];
            string valueType = parts[^1];

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(settingId))
            {
                Frontend.ShowMessageBox("Setting name and setting ID are required.");
                return false;
            }

            ResultEntries.Add(
                new NvidiaEditorEntry
                {
                    Name = name,
                    SettingId = settingId,
                    Value = string.IsNullOrWhiteSpace(value) ? "0" : value,
                    ValueType = NormalizeValueType(valueType)
                }
            );

            return true;
        }

        private void ImportFromFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "NVIDIA Profile (*.nip)|*.nip",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
                return;

            NipTextBox.Text = File.ReadAllText(dialog.FileName);
            Tabs.SelectedIndex = 1;
        }

        private void ParseAndAdd_Click(object sender, RoutedEventArgs e)
        {
            bool success = Tabs.SelectedIndex switch
            {
                0 => TryAddSingle(),
                1 => TryImportNip(),
                _ => TryAddFullValue()
            };

            if (!success)
                return;

            DialogResult = true;
            Close();
        }

        private bool TryImportNip()
        {
            try
            {
                var document = XDocument.Parse(NipTextBox.Text);

                var imported = document
                    .Descendants()
                    .Where(node => node.Name.LocalName.Equals("ProfileSetting", StringComparison.OrdinalIgnoreCase))
                    .Select(node =>
                    {
                        string id = node.Elements().FirstOrDefault(element => element.Name.LocalName.Equals("SettingID", StringComparison.OrdinalIgnoreCase))?.Value?.Trim() ?? "";
                        string name = node.Elements().FirstOrDefault(element => element.Name.LocalName.Equals("SettingNameInfo", StringComparison.OrdinalIgnoreCase))?.Value?.Trim() ?? "";
                        string value = node.Elements().FirstOrDefault(element => element.Name.LocalName.Equals("SettingValue", StringComparison.OrdinalIgnoreCase))?.Value?.Trim() ?? "0";
                        string valueType = NormalizeValueType(node.Elements().FirstOrDefault(element => element.Name.LocalName.Equals("ValueType", StringComparison.OrdinalIgnoreCase))?.Value);

                        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                            return null;

                        return new NvidiaEditorEntry
                        {
                            SettingId = id,
                            Name = name,
                            Value = value,
                            ValueType = valueType
                        };
                    })
                    .Where(entry => entry is not null)
                    .DistinctBy(entry => entry!.SettingId)
                    .Cast<NvidiaEditorEntry>()
                    .ToList();

                if (!imported.Any())
                {
                    Frontend.ShowMessageBox("No valid NVIDIA settings were found in that NIP file.");
                    return false;
                }

                ResultEntries.AddRange(imported);
                return true;
            }
            catch (Exception ex)
            {
                Frontend.ShowMessageBox($"Invalid or corrupted NIP file:\n\n{ex.Message}");
                return false;
            }
        }

        private static string NormalizeValueType(string? type)
        {
            return type?.Trim().ToLowerInvariant() switch
            {
                "dword" => "Dword",
                "string" => "String",
                "binary" => "Binary",
                "boolean" => "Boolean",
                "bool" => "Boolean",
                "hex" => "Hex",
                _ => "Dword"
            };
        }
    }
}
