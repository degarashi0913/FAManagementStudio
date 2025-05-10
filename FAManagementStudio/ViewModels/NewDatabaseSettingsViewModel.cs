using FAManagementStudio.Common;
using FAManagementStudio.ViewModels.Commons;
using FirebirdSql.Data.FirebirdClient;
using System.IO;
using System.Windows.Forms;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels;

public class NewDatabaseSettingsViewModel : ViewModelBase
{

    public NewDatabaseSettingsViewModel()
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
            using var dialog = new SaveFileDialog()
            {
                FileName = "NEWDB.FDB",
                DefaultExt = "fdb",
                Filter = "すべてのファイル(*.*)|*.*",
                OverwritePrompt = false
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;
            if (File.Exists(dialog.FileName)) return;
            Path = dialog.FileName;
            RaisePropertyChanged(nameof(Path));
        });
    }

    public string Path { get; set; } = string.Empty;
    public FirebirdType Type { get; set; } = FirebirdType.Fb3;
    public FbCharset CharSet { get; set; } = FbCharset.Utf8;

    public ICommand OkCommand { get; private set; }
    public ICommand CancelCommand { get; private set; }
    public ICommand OpenFileDialog { get; private set; }
}
