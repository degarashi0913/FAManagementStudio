using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace FAManagementStudio.ViewModels
{
    public class QueryResultViewModel : BindableBase
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

        private ObservableCollection<ResultDetailViewModel> _result = new ObservableCollection<ResultDetailViewModel>();
        public ObservableCollection<ResultDetailViewModel> Result
        {
            get { return _result; }
        }

        public QueryResultViewModel(string header)
        {
            Header = header;
        }

        public void GetExecuteResult(QueryInfo inf, string connectionString, string query)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                foreach (var queryResult in inf.ExecuteQuery(connectionString, query))
                {
                    var vm = new ResultDetailViewModel();
                    vm.View = queryResult.View;
                    vm.SetAdditionalInfo(queryResult.Count, queryResult.ExecuteTime, queryResult.ExecuteSql);
                    Result.Add(vm);
                }
            }));
        }

    }
    public class ResultDetailViewModel
    {
        public DataTable View { get; set; }
        public string AdditionalInfo { get; set; }

        public void SetAdditionalInfo(int count, TimeSpan time, string query)
        {
            AdditionalInfo = "";
            if (0 < count)
            {
                AdditionalInfo += $"取得行: {count} ";
            }
            if (0 < time.TotalSeconds)
            {
                AdditionalInfo += $"実行時間: {time.TotalSeconds}秒 ";
            }
            AdditionalInfo += query;
        }
    }
}
