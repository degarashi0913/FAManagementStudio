﻿using FAManagementStudio.Common;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public class QueryTabViewModel : BindableBase
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

        public QueryTabViewModel(string header, string query)
        {
            _header = header;
            _query = query;
            LoadQuery = new RelayCommand(() => LoadQueryMethod());
            GivingNameSave = new RelayCommand(() => GivingNameSaveMethod());
            OverwriteSave = new RelayCommand(() => OverwriteSaveMethod());
        }

        public static QueryTabViewModel GetNewInstance()
        {
            return new QueryTabViewModel("+", "");
        }

        public ICommand LoadQuery { get; private set; }
        public ICommand GivingNameSave { get; private set; }
        public ICommand OverwriteSave { get; private set; }

        private string _loadPath = "";

        private void LoadQueryMethod()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.DefaultExt = "txt";
                dialog.Filter = "txt files (*.txt)|*.txt|すべてのファイル(*.*)|*.*";
                if (dialog.ShowDialog() != DialogResult.OK) return;
                _loadPath = dialog.FileName;
            }
            using (var stream = new StreamReader(_loadPath, Encoding.UTF8))
            {
                Query = stream.ReadToEnd();
            }
        }

        private void GivingNameSaveMethod()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.FileName = $"{Header}.txt";
                dialog.DefaultExt = "txt";
                dialog.Filter = "txt files (*.txt)|*.txt|すべてのファイル(*.*)|*.*";
                if (dialog.ShowDialog() != DialogResult.OK) return;
                _loadPath = dialog.FileName;
            }
            SaveQuery(_loadPath);
        }

        private void OverwriteSaveMethod()
        {
            if (string.IsNullOrEmpty(_loadPath))
            {
                GivingNameSaveMethod();
                return;
            }
            SaveQuery(_loadPath);
        }

        private void SaveQuery(string path)
        {
            using (var stream = new StreamWriter(path, false, Encoding.UTF8))
            {
                stream.Write(Query);
            }
        }
    }
}

