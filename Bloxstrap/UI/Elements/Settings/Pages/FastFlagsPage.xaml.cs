using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Bloxstrap.UI.ViewModels.Settings;
using Wpf.Ui.Mvvm.Contracts;

namespace Bloxstrap.UI.Elements.Settings.Pages
{
    /// <summary>
    /// Interaction logic for FastFlagsPage.xaml
    /// </summary>
    public partial class FastFlagsPage
    {
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
            _viewModel = new FastFlagsViewModel();

            _viewModel.OpenFlagEditorEvent += OpenFlagEditor;
            _viewModel.RequestPageReloadEvent += (_, _) => SetupViewModel();

            DataContext = _viewModel;
        }

        private void OpenFlagEditor(object? sender, EventArgs e)
        {
            if (Window.GetWindow(this) is INavigationWindow window)
            {
               window.Navigate(typeof(FastFlagEditorPage));
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // refresh datacontext on page load to synchronize with editor page
            
            if (!_initialLoad)
            {
                _initialLoad = true;
                LoadSavedPresetSelection();
                return;
            }

            SetupViewModel();
            LoadSavedPresetSelection();
        }

        private void InfoBar_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Utilities.ShellExecute("https://devforum.roblox.com/t/allowlist-for-local-client-configuration-via-fast-flags/3966569");
        }

        private void ApplyPresetButton_Click(object sender, RoutedEventArgs e)
        {
            ComboBox? comboBox = FindPresetComboBox();

            if (comboBox?.SelectedItem is not ComboBoxItem item || item.Content is not string presetName)
                return;

            switch (presetName)
            {
                case "Ultra Low Delay":
                    _presetViewModel.ApplyUltraLowDelayPresetCommand.Execute(null);
                    break;
                case "Poddoks Fast Flags":
                    _presetViewModel.ApplyAbsoluteMaxFpsMinDelayCommand.Execute(null);
                    break;
            }

            SetupViewModel();
        }

        private void LoadSavedPresetSelection()
        {
            string savedPreset = App.Settings.Prop.LastSelectedFastFlagPreset;

            ComboBox? comboBox = FindPresetComboBox();

            if (comboBox is null)
                return;

            var presetItem = comboBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(x => String.Equals(x.Content as string, savedPreset, StringComparison.Ordinal));

            comboBox.SelectedItem = presetItem ?? comboBox.Items.OfType<ComboBoxItem>().FirstOrDefault();
        }

        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ComboBox comboBox || comboBox.SelectedItem is not ComboBoxItem item || item.Content is not string presetName)
                return;

            App.Settings.Prop.LastSelectedFastFlagPreset = presetName;
            App.Settings.Save();
        }

        private ComboBox? FindPresetComboBox() => FindDescendant(this, comboBox => String.Equals(comboBox.Tag as string, "PresetFastFlags", StringComparison.Ordinal));

        private static ComboBox? FindDescendant(DependencyObject parent, Func<ComboBox, bool> predicate)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is ComboBox match && predicate(match))
                    return match;

                ComboBox? nested = FindDescendant(child, predicate);

                if (nested is not null)
                    return nested;
            }

            return null;
        }

        private void ValidateInt32(object sender, TextCompositionEventArgs e) => e.Handled = e.Text != "-" && !Int32.TryParse(e.Text, out int _);
        
        private void ValidateUInt32(object sender, TextCompositionEventArgs e) => e.Handled = !UInt32.TryParse(e.Text, out uint _);
    }
}
