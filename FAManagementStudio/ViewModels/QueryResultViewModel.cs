using FAManagementStudio.Common;
using FAManagementStudio.Foundation.Common;
using FAManagementStudio.Models;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels;

public class QueryResultViewModel : BindableBase
{
    private string _header = string.Empty;
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
    public ObservableCollection<ResultDetailViewModel> Result { get; } = [];

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
                    var vm = ResultDetailViewModel.CreateResultDetailViewModel(queryResult);
                    Result.Add(vm);
                }
            }
            catch (Exception e)
            {

                var table = new DataTable("Exception");
                table.Columns.Add(new DataColumn { ColumnName = "0", Caption = "Message", DataType = typeof(string) });
                table.Rows.Add(e.Message);
                var vm = new ResultDetailViewModel(table, string.Empty);
                Result.Add(vm);
            }
        }));
    }

}
public class ResultDetailViewModel
{
    public DataTable View { get; }
    public string AdditionalInfo { get; }

    public ICommand OutputCsv { get; private set; }

    public ResultDetailViewModel(DataTable view, string additionalInfo)
    {
        View = view;
        AdditionalInfo = additionalInfo;
        OutputCsv = new RelayCommand<bool>(OnOutputCsv);
    }

    public static ResultDetailViewModel CreateResultDetailViewModel(QueryResult queryResult)
    {
        var info = GetAdditionalInfoString(queryResult.Count, queryResult.ExecuteTime, queryResult.ExecuteSql, queryResult.ExecutionPlan);
        return new ResultDetailViewModel(queryResult.View, info);
    }

    private static string GetAdditionalInfoString(int count, TimeSpan time, string query, string executionPlan)
    {
        var sb = new StringBuilder();

        if (0 < count)
        {
            sb.Append($"取得行: {count} ");
        }
        if (0 < time.TotalSeconds)
        {
            sb.Append($"実行時間: {time.TotalSeconds}秒 ");
        }
        sb.Append(query);
        if (!string.IsNullOrEmpty(executionPlan))
        {
            sb.Append(Environment.NewLine + $"実行プラン: {executionPlan}");
        }
        return sb.ToString();
    }

    private void OnOutputCsv(bool needHeader)
    {
        static (bool IsOk, string Path) OpenSaveFileDialog()
        {
            using var dialog = new SaveFileDialog()
            {
                FileName = "output.csv",
                DefaultExt = "csv",
                Filter = "csv files (*.csv)|*.csv|すべてのファイル(*.*)|*.*"
            };
            return (dialog.ShowDialog() == DialogResult.OK, dialog.FileName);
        }

        if (OpenSaveFileDialog() is { } result && !result.IsOk) return;

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
            File.WriteAllText(result.Path, sb.ToString(), Encoding.UTF8);
        }
        catch (Exception ex)
        {
            //throwしても拾う先が無いので握りつぶす
            MessageBox.Show(ex.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
