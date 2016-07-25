using FAManagementStudio.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public class NewDatabaseSettingsViewModel : ViewModelBase
    {

        public NewDatabaseSettingsViewModel()
        {
            SetCommand();
        }

        public string Path { get; set; }
        public FirebirdType Type {
            get;
            set; }

        public ICommand OkCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand OpenFileDialog { get; private set; }
        private void SetCommand()
        {
            OkCommand = new RelayCommand(() =>
            {
                MessengerInstance.Send(new MessageBase(this, "WindowClose"));
            });
            CancelCommand = new RelayCommand(() =>
            {
                Path = "";
                MessengerInstance.Send(new MessageBase(this, "WindowClose"));
            });
            OpenFileDialog = new RelayCommand(() =>
            {
                using (var dialog = new SaveFileDialog())
                {
                    dialog.FileName = "NEW_DB.fdb";
                    dialog.DefaultExt = "fdb";
                    dialog.Filter = "すべてのファイル(*.*)|*.*";
                    dialog.OverwritePrompt = false;
                    if (dialog.ShowDialog() != DialogResult.OK) return;
                    if (File.Exists(dialog.FileName)) return;
                    Path = dialog.FileName;
                    RaisePropertyChanged(nameof(Path));
                }
            });
        }
    }
}
