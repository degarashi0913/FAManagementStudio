using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

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

        private bool _pined = false;
        public bool Pined
        {
            get { return _pined; }
            set
            {
                _pined = value;
                RaisePropertyChanged(nameof(Pined));
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
            System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                try
                {
                    foreach (var queryResult in inf.ExecuteQuery(connectionString, query))
                    {
                        var vm = new ResultDetailViewModel();
                        vm.View = queryResult.View;
                        vm.SetAdditionalInfo(queryResult.Count, queryResult.ExecuteTime, queryResult.ExecuteSql, queryResult.ExecutionPlan);
                        Result.Add(vm);
                    }
                }
                catch (Exception e)
                {
                    var vm = new ResultDetailViewModel();
                    var table = new DataTable("Exception");
                    table.Columns.Add(new DataColumn { ColumnName = "0", Caption = "Message", DataType = typeof(string) });
                    table.Rows.Add(e.Message);
                    vm.View = table;
                    Result.Add(vm);
                }
            }));
        }

    }
    public class ResultDetailViewModel
    {
        public DataTable View { get; set; }
        public string AdditionalInfo { get; set; }

        public ICommand OutputCsv { get; private set; }

        public ResultDetailViewModel()
        {
            OutputCsv = new RelayCommand<bool>((needHeader) =>
            {
                var path = "";
                using (var dialog = new SaveFileDialog())
                {
                    dialog.FileName = "output.csv";
                    dialog.DefaultExt = "csv";
                    dialog.Filter = "csv files (*.csv)|*.csv|すべてのファイル(*.*)|*.*";
                    if (dialog.ShowDialog() != DialogResult.OK) return;
                    path = dialog.FileName;
                }

                var sb = new StringBuilder();
                if (needHeader)
                {
                    var header = View.Columns.Cast<DataColumn>().Select(x => $"\"{x.Caption}\"").ToArray();
                    sb.AppendLine(string.Join(",", header));
                }
                var rows = View.Rows.Cast<DataRow>().Select(x => string.Join(",", x.ItemArray.Select(y => $"\"{y}\"").ToArray()));
                foreach (var row in rows)
                {
                    sb.AppendLine(row);
                }
                try
                {
                    File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    //throwしても拾う先が無いので握りつぶす
                    MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        public void SetAdditionalInfo(int count, TimeSpan time, string query, string executionPlan)
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
            if (!string.IsNullOrEmpty(executionPlan))
            {
                AdditionalInfo += Environment.NewLine + $"実行プラン: {executionPlan}";
            }
        }
    }
}
