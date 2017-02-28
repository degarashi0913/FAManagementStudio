using FAManagementStudio.Common;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public class QueryTabViewModel : ViewModelBase
    {
        private string _header;
        public string Header
        {
            get { return _header; }
            set
            {
                _header = value;
                RaisePropertyChanged(nameof(Header));
            }
        }
        private string _query;
        public string Query
        {
            get { return _query; }
            set
            {
                _query = value;
                RaisePropertyChanged(nameof(Query));
            }
        }
        private bool _isNewResult = false;
        public bool IsNewResult
        {
            get { return _isNewResult; }
            set
            {
                _isNewResult = value;
                RaisePropertyChanged(nameof(IsNewResult));
            }
        }
        private bool _isShowExecutionPlan = false;
        public bool IsShowExecutionPlan
        {
            get { return _isShowExecutionPlan; }
            set
            {
                _isShowExecutionPlan = value;
                RaisePropertyChanged(nameof(IsShowExecutionPlan));
            }
        }
        private bool _IntelisenseEnabled = false;
        public bool IntelisenseEnabled
        {
            get { return _IntelisenseEnabled; }
            set
            {
                _IntelisenseEnabled = value;
                RaisePropertyChanged(nameof(IntelisenseEnabled));
            }
        }
        public QueryTabViewModel(string header, string query)
        {
            _header = header;
            _query = query;
            LoadQuery = new RelayCommand(() => LoadQueryMethod());
            GivingNameSave = new RelayCommand(() => GivingNameSaveMethod());
            OverwriteSave = new RelayCommand(() => OverwriteSaveMethod());
            DropFile = new RelayCommand<string>(path =>
            {
                _loadPath = path;
                Query = FileLoad(_loadPath, _fileEncoding);
            });
        }

        public static QueryTabViewModel GetNewInstance()
        {
            return new QueryTabViewModel("+", "");
        }

        public ICommand LoadQuery { get; private set; }
        public ICommand GivingNameSave { get; private set; }
        public ICommand OverwriteSave { get; private set; }

        public ICommand DropFile { get; private set; }

        private Encoding _fileEncoding = Encoding.UTF8;

        private string _loadPath = "";

        private void LoadQueryMethod()
        {
            var path = "";
            using (var dialog = new OpenFileDialog())
            {
                dialog.DefaultExt = "fmq";
                dialog.Filter = "FamQuery (*.fmq)|*.fmq|txt files (*.txt)|*.txt|すべてのファイル(*.*)|*.*";
                if (dialog.ShowDialog() != DialogResult.OK) return;
                path = dialog.FileName;
            }
            Query = FileLoad(path, _fileEncoding);
        }

        public string FileLoad(string path, Encoding enc)
        {
            if (Path.GetExtension(path) == ".fmq")
            {
                _loadPath = path;
            }
            using (var stream = new StreamReader(path, enc))
            {
                return stream.ReadToEnd();
            }
        }

        private void GivingNameSaveMethod()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.FileName = $"{Header}.fmq";
                dialog.DefaultExt = "fmq";
                dialog.Filter = "FamQuery (*.fmq)|*.fmq|すべてのファイル(*.*)|*.*";
                if (dialog.ShowDialog() != DialogResult.OK) return;
                _loadPath = dialog.FileName;
            }
            SaveQuery(_loadPath, _fileEncoding);
        }

        private void OverwriteSaveMethod()
        {
            if (string.IsNullOrEmpty(_loadPath))
            {
                GivingNameSaveMethod();
                return;
            }
            SaveQuery(_loadPath, _fileEncoding);
        }

        private void SaveQuery(string path, Encoding enc)
        {
            using (var stream = new StreamWriter(path, false, enc))
            {
                stream.Write(Query);
            }
        }
    }
}

