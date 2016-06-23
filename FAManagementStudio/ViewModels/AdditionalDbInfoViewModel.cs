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
        public List<IAddtionalDbInfo> Content { get; }
        public AdditionalDbInfoViewModel(string name, List<IAddtionalDbInfo> content)
        {
            DisplayName = name;
            Content = content;
        }
    }

    public class AdditionalDbInfoControl : ViewModelBase
    {
        public object CurrentContent { get; private set; }
        public List<AdditionalDbInfoViewModel> ContentList { get; } = new List<AdditionalDbInfoViewModel>();
        public ICommand ContentChange { get; }
        public AdditionalDbInfoControl(DbViewModel db)
        {
            ContentChange = new RelayCommand<string>(x => ChangeContent(x));

            RefrechData(db);

            RaisePropertyChanged(nameof(CurrentContent));
        }

        public void RefrechData(DbViewModel db)
        {
            ContentList.Clear();

            ContentList.Add(new AdditionalDbInfoViewModel("Trigger", db.Triggers.Cast<IAddtionalDbInfo>().ToList()));
            ContentList.Add(new AdditionalDbInfoViewModel("Index", db.Indexes.Cast<IAddtionalDbInfo>().ToList()));
            ContentList.Add(new AdditionalDbInfoViewModel("Domain", db.Domains.Cast<IAddtionalDbInfo>().ToList()));

            CurrentContent = ContentList;
        }

        public void ChangeContent(string target)
        {
            var item = ContentList.Find(x => x.DisplayName == target);
            if (item == null)
            {
                CurrentContent = ContentList;
            }
            else
            {
                CurrentContent = item.Content;
            }
            RaisePropertyChanged(nameof(CurrentContent));
        }

    }

    public interface IAddtionalDbInfo { }
}
