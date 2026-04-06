using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;

using Microsoft.VisualBasic;

using Bloxstrap.Models.Persistable;
using Bloxstrap.UI.ViewModels.Settings;
using UiButton = Wpf.Ui.Controls.Button;
using Wpf.Ui.Mvvm.Contracts;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for FastFlagsPage.xaml
    /// </summary>
    public partial class FastFlagsPage
    {
        private enum CustomPresetActionMode
        {
            Save,
            Remove
        }

        private static readonly string[] BuiltInPresetNames =
        {
            "Ultra Low Delay",
            "Poddoks Fast Flags"
        };

        private static readonly string[] SavedGlobalSettingKeys =
        {
            "Rendering.FramerateCap",
            "Rendering.SavedQualityLevel",
            "UI.ReducedMotion"
        };

        private bool _initialLoad = false;
        private readonly GlobalSettingsViewModel _presetViewModel = new();

        private FastFlagsViewModel _viewModel = null!;

        public FastFlagsPage()
        {
            SetupViewModel();
            InitializeComponent();
        }

        private void SetupViewModel()
        {
            if (_viewModel is not null)
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;

            _viewModel = new FastFlagsViewModel();

            _viewModel.OpenFlagEditorEvent += OpenFlagEditor;
            _viewModel.RequestPageReloadEvent += (_, _) => SetupViewModel();
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;

            DataContext = _viewModel;
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RefreshPresetActionButton();
        }

        private void OpenFlagEditor(object? sender, EventArgs e)
        {
            if (Window.GetWindow(this) is INavigationWindow window)
                window.Navigate(typeof(FastFlagEditorPage));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // refresh datacontext on page load to synchronize with editor page
            if (!_initialLoad)
            {
                _initialLoad = true;
                RefreshPresetOptions();
                LoadSavedPresetSelection();
                RefreshPresetActionButton();
                return;
            }

            SetupViewModel();
            RefreshPresetOptions();
            LoadSavedPresetSelection();
            RefreshPresetActionButton();
        }

        private void InfoBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Utilities.ShellExecute("https://devforum.roblox.com/t/allowlist-for-local-client-configuration-via-fast-flags/3966569");
        }

        private void ApplyPresetButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetSelectedPresetName() is not string presetName)
                return;

            switch (presetName)
            {
                case "Ultra Low Delay":
                    _presetViewModel.ApplyUltraLowDelayPresetCommand.Execute(null);
                    break;
                case "Poddoks Fast Flags":
                    _presetViewModel.ApplyAbsoluteMaxFpsMinDelayCommand.Execute(null);
                    break;
                default:
                    ApplyCustomPreset(presetName);
                    break;
            }

            SetupViewModel();
            RefreshPresetOptions();
            SelectPreset(presetName);
            RefreshPresetActionButton();
        }

        private void CustomPresetActionButton_Click(object sender, RoutedEventArgs e)
        {
            switch (GetCustomPresetActionMode())
            {
                case CustomPresetActionMode.Remove:
                    RemoveCurrentPreset();
                    break;
                default:
                    SaveCurrentPreset();
                    break;
            }
        }

        private void SaveCurrentPreset()
        {
            string initialValue = GetSelectedPresetName() is string current && !IsBuiltInPreset(current) ? current : String.Empty;
            string presetName = Interaction.InputBox("Enter a name for this preset.", "Save Fast Flag Preset", initialValue).Trim();

            if (String.IsNullOrWhiteSpace(presetName))
                return;

            if (IsBuiltInPreset(presetName))
            {
                Frontend.ShowMessageBox("That preset name is reserved for a bundled preset. Choose a different name.", MessageBoxImage.Warning);
                return;
            }

            if (App.Settings.Prop.CustomFastFlagPresets.ContainsKey(presetName))
            {
                var overwrite = Frontend.ShowMessageBox($"A custom preset named '{presetName}' already exists. Overwrite it?", MessageBoxImage.Question, MessageBoxButton.YesNo, MessageBoxResult.No);

                if (overwrite != MessageBoxResult.Yes)
                    return;
            }

            App.Settings.Prop.CustomFastFlagPresets[presetName] = CaptureCurrentPreset();
            App.Settings.Prop.LastSelectedFastFlagPreset = presetName;
            App.Settings.Save();

            RefreshPresetOptions();
            SelectPreset(presetName);
            RefreshPresetActionButton();

            Frontend.ShowMessageBox($"Saved custom preset '{presetName}'.", MessageBoxImage.Information);
        }

        private void RemoveCurrentPreset()
        {
            string? presetName = FindMatchingCustomPresetName();

            if (String.IsNullOrWhiteSpace(presetName))
                return;

            var confirm = Frontend.ShowMessageBox($"Remove custom preset '{presetName}'?", MessageBoxImage.Question, MessageBoxButton.YesNo, MessageBoxResult.No);

            if (confirm != MessageBoxResult.Yes)
                return;

            App.Settings.Prop.CustomFastFlagPresets.Remove(presetName);

            if (String.Equals(App.Settings.Prop.LastSelectedFastFlagPreset, presetName, StringComparison.Ordinal))
                App.Settings.Prop.LastSelectedFastFlagPreset = BuiltInPresetNames[0];

            App.Settings.Save();

            RefreshPresetOptions();
            SelectPreset(App.Settings.Prop.LastSelectedFastFlagPreset);
            RefreshPresetActionButton();

            Frontend.ShowMessageBox($"Removed custom preset '{presetName}'.", MessageBoxImage.Information);
        }

        private void LoadSavedPresetSelection()
        {
            SelectPreset(App.Settings.Prop.LastSelectedFastFlagPreset);
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox || comboBox.SelectedItem is not string presetName)
                return;

            App.Settings.Prop.LastSelectedFastFlagPreset = presetName;
            App.Settings.Save();
            RefreshPresetActionButton();
        }

        private void RefreshPresetOptions()
        {
            string? selectedPreset = GetSelectedPresetName() ?? App.Settings.Prop.LastSelectedFastFlagPreset;

            ComboBox? comboBox = FindPresetComboBox();

            if (comboBox is null)
                return;

            comboBox.ItemsSource = BuiltInPresetNames
                .Concat(App.Settings.Prop.CustomFastFlagPresets.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                .ToList();

            SelectPreset(selectedPreset);
            RefreshPresetActionButton();
        }

        private void SelectPreset(string? presetName)
        {
            ComboBox? comboBox = FindPresetComboBox();

            if (comboBox is null)
                return;

            if (!String.IsNullOrWhiteSpace(presetName) && comboBox.Items.Contains(presetName))
            {
                comboBox.SelectedItem = presetName;
                return;
            }

            comboBox.SelectedItem = comboBox.Items.OfType<string>().FirstOrDefault();
        }

        private string? GetSelectedPresetName() => FindPresetComboBox()?.SelectedItem as string;

        private CustomPresetActionMode GetCustomPresetActionMode()
        {
            return FindMatchingCustomPresetName() is null
                ? CustomPresetActionMode.Save
                : CustomPresetActionMode.Remove;
        }

        private void RefreshPresetActionButton()
        {
            UiButton? button = FindCustomPresetActionButton();

            if (button is null)
                return;

            string? matchingPreset = FindMatchingCustomPresetName();
            bool canRemove = !String.IsNullOrWhiteSpace(matchingPreset);

            button.Content = canRemove ? "Remove Current" : "Save Current";
            button.Appearance = canRemove
                ? Wpf.Ui.Common.ControlAppearance.Primary
                : Wpf.Ui.Common.ControlAppearance.Secondary;
            button.ClearValue(Control.BackgroundProperty);
            button.ClearValue(Control.BorderBrushProperty);
            button.ClearValue(Control.ForegroundProperty);
            button.ClearValue(UiButton.MouseOverBackgroundProperty);
            button.ClearValue(UiButton.MouseOverBorderBrushProperty);
            button.ClearValue(UiButton.PressedBackgroundProperty);
            button.ClearValue(UiButton.PressedBorderBrushProperty);
            button.ClearValue(UiButton.PressedForegroundProperty);
        }

        private static bool IsBuiltInPreset(string presetName) => BuiltInPresetNames.Contains(presetName, StringComparer.Ordinal);

        private static CustomFastFlagPreset CaptureCurrentPreset()
        {
            var preset = new CustomFastFlagPreset
            {
                FastFlags = App.FastFlags.Prop.ToDictionary(x => x.Key, x => x.Value?.ToString() ?? String.Empty, StringComparer.Ordinal),
                UseFastFlagManager = App.Settings.Prop.UseFastFlagManager,
                EnableBetterMatchmaking = App.Settings.Prop.EnableBetterMatchmaking,
                EnableBetterMatchmakingRandomization = App.Settings.Prop.EnableBetterMatchmakingRandomization,
                DisableTerrainTextures = App.Settings.Prop.DisableTerrainTextures,
                ExtremeStripMode = App.Settings.Prop.ExtremeStripMode,
                AutoRepairBrokenRobloxInstallBeforeLaunch = App.Settings.Prop.AutoRepairBrokenRobloxInstallBeforeLaunch,
                CleanupStaleRobloxFilesBeforeLaunch = App.Settings.Prop.CleanupStaleRobloxFilesBeforeLaunch
            };

            foreach (string key in SavedGlobalSettingKeys)
                preset.GlobalSettings[key] = App.GlobalSettings.GetPreset(key);

            return preset;
        }

        private static void ApplyCustomPreset(string presetName)
        {
            if (!App.Settings.Prop.CustomFastFlagPresets.TryGetValue(presetName, out var preset))
            {
                Frontend.ShowMessageBox($"Custom preset '{presetName}' could not be found.", MessageBoxImage.Warning);
                return;
            }

            App.Settings.Prop.UseFastFlagManager = preset.UseFastFlagManager;
            App.Settings.Prop.EnableBetterMatchmaking = preset.EnableBetterMatchmaking;
            App.Settings.Prop.EnableBetterMatchmakingRandomization = preset.EnableBetterMatchmakingRandomization;
            App.Settings.Prop.DisableTerrainTextures = preset.DisableTerrainTextures;
            App.Settings.Prop.ExtremeStripMode = preset.ExtremeStripMode;
            App.Settings.Prop.AutoRepairBrokenRobloxInstallBeforeLaunch = preset.AutoRepairBrokenRobloxInstallBeforeLaunch;
            App.Settings.Prop.CleanupStaleRobloxFilesBeforeLaunch = preset.CleanupStaleRobloxFilesBeforeLaunch;

            App.FastFlags.Prop = preset.FastFlags.ToDictionary(x => x.Key, x => (object)x.Value, StringComparer.Ordinal);

            foreach (string key in SavedGlobalSettingKeys)
                App.GlobalSettings.SetPreset(key, preset.GlobalSettings.TryGetValue(key, out var value) ? value : null);

            Frontend.ShowMessageBox($"Custom preset '{presetName}' applied. Click Save to keep these settings.", MessageBoxImage.Information);
        }

        private ComboBox? FindPresetComboBox() => FindComboBoxDescendant(this, comboBox => String.Equals(comboBox.Tag as string, "PresetFastFlags", StringComparison.Ordinal));

        private UiButton? FindCustomPresetActionButton() => FindButtonDescendant(this, button => String.Equals(button.Tag as string, "CustomPresetAction", StringComparison.Ordinal));

        private string? FindMatchingCustomPresetName()
        {
            CustomFastFlagPreset currentPreset = CaptureCurrentPreset();

            return App.Settings.Prop.CustomFastFlagPresets
                .FirstOrDefault(pair => PresetsEqual(currentPreset, pair.Value))
                .Key;
        }

        private static bool PresetsEqual(CustomFastFlagPreset left, CustomFastFlagPreset right)
        {
            return left.UseFastFlagManager == right.UseFastFlagManager
                && left.EnableBetterMatchmaking == right.EnableBetterMatchmaking
                && left.EnableBetterMatchmakingRandomization == right.EnableBetterMatchmakingRandomization
                && left.DisableTerrainTextures == right.DisableTerrainTextures
                && left.ExtremeStripMode == right.ExtremeStripMode
                && left.AutoRepairBrokenRobloxInstallBeforeLaunch == right.AutoRepairBrokenRobloxInstallBeforeLaunch
                && left.CleanupStaleRobloxFilesBeforeLaunch == right.CleanupStaleRobloxFilesBeforeLaunch
                && DictionariesEqual(left.FastFlags, right.FastFlags)
                && DictionariesEqual(left.GlobalSettings, right.GlobalSettings);
        }

        private static bool DictionariesEqual<TValue>(Dictionary<string, TValue> left, Dictionary<string, TValue> right)
        {
            if (left.Count != right.Count)
                return false;

            foreach (var pair in left)
            {
                if (!right.TryGetValue(pair.Key, out TValue? value) || !EqualityComparer<TValue>.Default.Equals(pair.Value, value))
                    return false;
            }

            return true;
        }

        private static ComboBox? FindComboBoxDescendant(DependencyObject parent, Func<ComboBox, bool> predicate)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is ComboBox match && predicate(match))
                    return match;

                ComboBox? nested = FindComboBoxDescendant(child, predicate);

                if (nested is not null)
                    return nested;
            }

            return null;
        }

        private static UiButton? FindButtonDescendant(DependencyObject parent, Func<UiButton, bool> predicate)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is UiButton match && predicate(match))
                    return match;

                UiButton? nested = FindButtonDescendant(child, predicate);

                if (nested is not null)
                    return nested;
            }

            return null;
        }

        private void ValidateInt32(object sender, TextCompositionEventArgs e) => e.Handled = e.Text != "-" && !Int32.TryParse(e.Text, out int _);

        private void ValidateUInt32(object sender, TextCompositionEventArgs e) => e.Handled = !UInt32.TryParse(e.Text, out uint _);
    }
}
