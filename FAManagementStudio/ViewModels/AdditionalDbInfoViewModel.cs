using FAManagementStudio.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public class AdditionalDbInfoViewModel : ViewModelBase
    {
        public AdditionalDbInfoViewModel() { }
        public string DisplayName { get; }
        public AdditionalDbInfoViewModel(string name)
        {
            DisplayName = name;
        }
    }

    public class AdditionalDbInfoControl : ViewModelBase
    {
        public object CurrentContent { get; private set; }
        public List<AdditionalDbInfoViewModel> ContentList { get; } = new List<AdditionalDbInfoViewModel> {
            new AdditionalDbInfoViewModel("Triggers") ,new AdditionalDbInfoViewModel("Indexs"),
            new AdditionalDbInfoViewModel("Domains"), new AdditionalDbInfoViewModel("Procedures"),
            new AdditionalDbInfoViewModel("Sequences")};
        public ICommand ContentChange { get; }
        private DbViewModel _db;
        public AdditionalDbInfoControl(DbViewModel db)
        {
            ContentChange = new RelayCommand<string>(x => ChangeContent(x));
            _db = db;
            CurrentContent = ContentList;
            RaisePropertyChanged(nameof(CurrentContent));
        }

        public void ChangeContent(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                CurrentContent = ContentList;
            }
            else if (target == "Triggers")
            {
                CurrentContent = _db.Triggers;
            }
            else if (target == "Indexs")
            {
                CurrentContent = _db.Indexes;
            }
            else if (target == "Domains")
            {
                CurrentContent = _db.Domains;
            }
            else if (target == "Procedures")
            {
                CurrentContent = _db.Procedures;
            }
            else if (target == "Sequences")
            {
                CurrentContent = _db.Sequences;
            }
            RaisePropertyChanged(nameof(CurrentContent));
        }

    }

    public interface IAddtionalDbInfo { }
}
