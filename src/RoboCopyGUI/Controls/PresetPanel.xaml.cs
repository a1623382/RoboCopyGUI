using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using RoboCopyGUI.Models;
using RoboCopyGUI.ViewModels;

namespace RoboCopyGUI.Controls;

public sealed partial class PresetPanel : UserControl
{
    public PresetPanel()
    {
        InitializeComponent();
    }

    private MainViewModel? GetViewModel()
    {
        return DataContext as MainViewModel;
    }

    private async void SavePreset_Click(object sender, RoutedEventArgs e)
    {
        var vm = GetViewModel();
        if (vm is null) return;

        vm.NewPresetName = TxtPresetName.Text;
        vm.NewPresetDescription = TxtPresetDesc.Text;
        await vm.SavePresetCommand.ExecuteAsync(null);

        TxtPresetName.Text = string.Empty;
        TxtPresetDesc.Text = string.Empty;
    }

    private async void DeletePreset_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn || btn.Tag is not string name) return;
        var vm = GetViewModel();
        if (vm is null) return;

        var preset = vm.Presets.FirstOrDefault(p => p.Name == name);
        if (preset is not null)
            await vm.DeletePresetCommand.ExecuteAsync(preset);
    }

    private void ApplyPreset_Click(object sender, RoutedEventArgs e)
    {
        var vm = GetViewModel();
        if (vm?.SelectedPreset is null || vm.SelectedTask is null) return;
        vm.ApplyPresetCommand.Execute(vm.SelectedTask);
    }
}
