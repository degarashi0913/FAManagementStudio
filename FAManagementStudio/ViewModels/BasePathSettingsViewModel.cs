using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public class BasePathSettingsViewModel : ViewModelBase
    {
        public BasePathSettingsViewModel() { }
        public BasePathSettingsViewModel(IList<IProjectNodeViewModel> vm)
        {
            _vm = vm;
            SetCommand();
        }
        private IList<IProjectNodeViewModel> _vm;
        public IList<IProjectNodeViewModel> Items
        {
            get { return _vm; }
        }
        public IProjectNodeViewModel SelectedItem { get; set; }

        public ICommand AddBasePath { get; private set; }
        public ICommand DeletePath { get; private set; }
        public ICommand CloseWindow { get; private set; }

        private void SetCommand()
        {
            AddBasePath = new RelayCommand(() =>
            {
                using (var dlg = new FolderBrowserDialog())
                {
                    if (dlg.ShowDialog() != DialogResult.OK) return;
                    foreach (var item in QueryProjectViewModel.GetData(dlg.SelectedPath))
                    {
                        _vm.Add(item);
                        AppSettingsManager.QueryProjectBasePaths.Add(item.FullPath);
                    }
                }
            });

            DeletePath = new RelayCommand(() =>
            {
                if (SelectedItem == null) return;
                AppSettingsManager.QueryProjectBasePaths.Remove(SelectedItem.FullPath);
                _vm.Remove(SelectedItem);
            });

            CloseWindow = new RelayCommand(() =>
            {
                MessengerInstance.Send(new MessageBase(this, "WindowClose"));
            });
        }
    }
}
