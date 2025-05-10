using FAManagementStudio.Common;
using FAManagementStudio.Models;
using FAManagementStudio.ViewModels.Commons;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels;

public class BasePathSettingsViewModel : ViewModelBase
{
    /// <summary>
    /// Default constructor for design time data.
    /// </summary>
#pragma warning disable CS8618
    public BasePathSettingsViewModel() { }
#pragma warning restore CS8618 
    public BasePathSettingsViewModel(IList<IProjectNodeViewModel> vm)
    {
        _vm = vm;
    }
    private readonly IList<IProjectNodeViewModel> _vm;
    public IList<IProjectNodeViewModel> Items => _vm;
    public IProjectNodeViewModel? SelectedItem { get; set; }

    private ICommand? _addBaseCommand;
    public ICommand AddBasePath => _addBaseCommand ??= new RelayCommand(OnAddBasePath);
    private ICommand? _deletePashCommand;
    public ICommand DeletePath => _deletePashCommand ??= new RelayCommand(OnDeletePath);
    private ICommand? _closeWindowCommand;
    public ICommand CloseWindow => _closeWindowCommand ??= new RelayCommand(OnCloseWindow);

    private void OnAddBasePath()
    {
        using var dlg = new FolderBrowserDialog();
        if (dlg.ShowDialog() != DialogResult.OK) return;
        foreach (var item in QueryProjectViewModel.GetData(dlg.SelectedPath))
        {
            _vm.Add(item);
            AppSettingsManager.QueryProjectBasePaths.Add(item.FullPath);
        }
    }

    private void OnDeletePath()
    {
        if (SelectedItem == null) return;
        AppSettingsManager.QueryProjectBasePaths.Remove(SelectedItem.FullPath);
        _vm.Remove(SelectedItem);
    }

    private void OnCloseWindow()
    {
        MessengerInstance.Send(new MessageBase(this, "WindowClose"));
    }
}
