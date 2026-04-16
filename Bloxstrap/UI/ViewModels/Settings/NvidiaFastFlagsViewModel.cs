using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Bloxstrap.Integrations;
using Bloxstrap.Models;

namespace Bloxstrap.UI.ViewModels.Settings
{
    public sealed class NvidiaFastFlagsViewModel : INotifyPropertyChanged
    {
        private static readonly string NipPath = Path.Combine(Paths.Integrations, "Nvidia", "Crystrap.nip");

        private static readonly Dictionary<string, int> BenchmarkOverlayMap = new()
        {
            { "Disabled", 0 },
            { "GRAPH_FLIP_FPS - FPS graph, measured on display hw flip", 1 },
            { "GRAPH_PRESENT_FPS - FPS graph, measured when the user mode driver starts processing present", 2 },
            { "GRAPH_APP_PRESENT_FPS - FPS graph, measured on app present", 4 },
            { "DISPLAY_PAGING - Add red paging indicator bars to the GRAPH_PRESENT_FPS graph", 8 },
            { "DISPLAY_APP_THREAD_WAIT - Add app thread wait time bars to the GRAPH_APP_PRESENT_FPS graph", 16 },
            { "Enabled - Enable everything", 511 }
        };

        private string _selectedCplLowLatencyMode = "Off";
        private string _benchmarkOverlayMode = "Disabled";
        private string _selectedFrlLowLatencyMode = "Off";
        private string _selectedSilkSmoothness = "Off";
        private bool _enableRbar;
        private bool _enableGamma;
        private bool _enableMFAA;
        private bool _enableFXAA;
        private int _textureLodBias;
        private int _frameRateLimit;
        private int _backgroundFrameRateLimit;
        private Dictionary<string, string> _originalValues = new();

        public ObservableCollection<string> CplLowLatencyModes { get; } = new() { "Off", "On", "Ultra" };
        public ObservableCollection<string> BenchMarkOverlayModes { get; } = new(BenchmarkOverlayMap.Keys);
        public ObservableCollection<string> FrlLowLatencyModes { get; } = new() { "Off", "On" };
        public ObservableCollection<string> SilkSmoothnessModes { get; } = new() { "Off", "Low", "Medium", "High", "Ultra" };

        public string SelectedCplLowLatencyMode
        {
            get => _selectedCplLowLatencyMode;
            set => Set(ref _selectedCplLowLatencyMode, value);
        }

        public string BenchMarkOverlayMode
        {
            get => _benchmarkOverlayMode;
            set => Set(ref _benchmarkOverlayMode, value);
        }

        public string SelectedFrlLowLatencyMode
        {
            get => _selectedFrlLowLatencyMode;
            set => Set(ref _selectedFrlLowLatencyMode, value);
        }

        public string SelectedSilkSmoothness
        {
            get => _selectedSilkSmoothness;
            set => Set(ref _selectedSilkSmoothness, value);
        }

        public bool EnableRbar
        {
            get => _enableRbar;
            set => Set(ref _enableRbar, value);
        }

        public bool EnableGamma
        {
            get => _enableGamma;
            set => Set(ref _enableGamma, value);
        }

        public bool EnableMFAA
        {
            get => _enableMFAA;
            set => Set(ref _enableMFAA, value);
        }

        public bool EnableFXAA
        {
            get => _enableFXAA;
            set => Set(ref _enableFXAA, value);
        }

        public int FrameRateLimit
        {
            get => _frameRateLimit;
            set => Set(ref _frameRateLimit, Math.Clamp(value, 0, 1000));
        }

        public int BackgroundFrameRateLimit
        {
            get => _backgroundFrameRateLimit;
            set => Set(ref _backgroundFrameRateLimit, Math.Clamp(value, 0, 1000));
        }

        public int TextureLodBias
        {
            get => _textureLodBias;
            set
            {
                Set(ref _textureLodBias, Math.Clamp(value, 0, 120));
                OnPropertyChanged(nameof(TextureLodBiasLabel));
            }
        }

        public string TextureLodBiasLabel => TextureLodBias == 0
            ? "Default (driver controlled)"
            : $"LOD bias override: {TextureLodBias}";

        public NvidiaFastFlagsViewModel()
        {
            EnsureNipExists();
            Load();
        }

        public async Task Apply()
        {
            var entries = RemoveDuplicateSettingIds(NvidiaProfileManager.LoadFromNip(NipPath));

            ApplyIfChanged(entries, "Frame Rate Limiter", "277041154", FrameRateLimit.ToString());
            ApplyIfChanged(entries, "Background Application Max Frame Rate", "277041157", BackgroundFrameRateLimit.ToString());
            ApplyIfChanged(entries, "CPL Low Latency Mode", "390467", CplLowLatencyModes.IndexOf(SelectedCplLowLatencyMode).ToString());
            ApplyIfChanged(entries, "Benchmark Overlay", "2945366", BenchmarkOverlayMap.TryGetValue(BenchMarkOverlayMode, out var overlay) ? overlay.ToString() : "0");
            ApplyIfChanged(entries, "FRL Low Latency Mode", "277041152", FrlLowLatencyModes.IndexOf(SelectedFrlLowLatencyMode).ToString());
            ApplyIfChanged(entries, "SILK Smoothness", "9990737", SilkToValue(SelectedSilkSmoothness));
            ApplyIfChanged(entries, "Resizable BAR", "549198379", EnableRbar ? "1" : "0");

            string gammaValue = EnableGamma ? "0" : "1";
            ApplyIfChanged(entries, "Gamma correction", "276652957", gammaValue);
            ApplyIfChanged(entries, "Line gamma", "545898348", gammaValue);

            string fxaaValue = EnableFXAA ? "1" : "0";
            ApplyIfChanged(entries, "Enable FXAA", "276089202", fxaaValue);
            ApplyIfChanged(entries, "Antialiasing - Mode", "276757595", fxaaValue);

            ApplyIfChanged(entries, "MFAA", "10011052", EnableMFAA ? "1" : "0");
            ApplyIfChanged(entries, "Texture filtering - LOD Bias", "7573135", TextureLodBias.ToString());

            if (TextureLodBias > 0)
            {
                ApplyIfChanged(entries, "Texture filtering - Quality", "13510289", "20");
                ApplyIfChanged(entries, "Anisotropic filtering mode", "282245910", "1");
                ApplyIfChanged(entries, "Antialiasing - Transparency Supersampling", "282364549", "8");
            }

            NvidiaProfileManager.SaveToNip(NipPath, entries);
            _originalValues = entries.ToDictionary(entry => entry.SettingId, entry => entry.Value);
            await NvidiaProfileManager.ApplyNipFile(NipPath);
        }

        private void Load()
        {
            var entries = RemoveDuplicateSettingIds(NvidiaProfileManager.LoadFromNip(NipPath));

            SelectedCplLowLatencyMode = ReadEnum(entries, "390467", CplLowLatencyModes, 0);
            BenchMarkOverlayMode = BenchmarkOverlayMap.FirstOrDefault(pair => pair.Value == ReadInt(entries, "2945366", 0)).Key ?? "Disabled";
            SelectedFrlLowLatencyMode = ReadEnum(entries, "277041152", FrlLowLatencyModes, 0);
            SelectedSilkSmoothness = ReadSilk(entries);
            EnableRbar = ReadBool(entries, "549198379");
            FrameRateLimit = ReadInt(entries, "277041154", 0);
            BackgroundFrameRateLimit = ReadInt(entries, "277041157", 0);
            EnableMFAA = ReadBool(entries, "10011052");
            EnableFXAA = ReadBool(entries, "276089202") && ReadBool(entries, "276757595");
            EnableGamma = !(ReadBool(entries, "276652957") && ReadBool(entries, "545898348"));
            TextureLodBias = ReadInt(entries, "7573135", 0);
            _originalValues = entries.ToDictionary(entry => entry.SettingId, entry => entry.Value);
        }

        private static void EnsureNipExists()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(NipPath)!);

            if (!File.Exists(NipPath))
                File.WriteAllText(NipPath, NvidiaProfileManager.EmptyNipTemplate());
        }

        private void ApplyIfChanged(List<NvidiaEditorEntry> entries, string name, string id, string newValue)
        {
            _originalValues.TryGetValue(id, out string? oldValue);

            if (oldValue == newValue)
                return;

            if (newValue == "0")
            {
                entries.RemoveAll(entry => entry.SettingId == id);
                return;
            }

            var existing = entries.FirstOrDefault(entry => entry.SettingId == id);

            if (existing is not null)
            {
                existing.Value = newValue;
                return;
            }

            entries.Add(
                new NvidiaEditorEntry
                {
                    Name = name,
                    SettingId = id,
                    Value = newValue,
                    ValueType = "Dword"
                }
            );
        }

        private static bool ReadBool(List<NvidiaEditorEntry> entries, string id)
            => entries.FirstOrDefault(entry => entry.SettingId == id)?.Value == "1";

        private static int ReadInt(List<NvidiaEditorEntry> entries, string id, int defaultValue)
            => int.TryParse(entries.FirstOrDefault(entry => entry.SettingId == id)?.Value, out int value) ? value : defaultValue;

        private static string ReadEnum(List<NvidiaEditorEntry> entries, string id, IList<string> values, int defaultIndex)
            => int.TryParse(entries.FirstOrDefault(entry => entry.SettingId == id)?.Value, out int value) && value < values.Count
                ? values[value]
                : values[defaultIndex];

        private static string ReadSilk(List<NvidiaEditorEntry> entries)
        {
            return entries.FirstOrDefault(entry => entry.SettingId == "9990737")?.Value switch
            {
                "1" => "Low",
                "2" => "Medium",
                "3" => "High",
                "4" => "Ultra",
                _ => "Off"
            };
        }

        private static string SilkToValue(string mode)
        {
            return mode switch
            {
                "Low" => "1",
                "Medium" => "2",
                "High" => "3",
                "Ultra" => "4",
                _ => "0"
            };
        }

        private static List<NvidiaEditorEntry> RemoveDuplicateSettingIds(List<NvidiaEditorEntry> entries)
            => entries.GroupBy(entry => entry.SettingId).Select(group => group.First()).ToList();

        private void Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return;

            field = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
