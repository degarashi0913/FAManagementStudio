﻿using FAManagementStudio.Common;
using FAManagementStudio.Models;
using FirebirdSql.Data.FirebirdClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace FAManagementStudio.ViewModels
{
    public class DbViewModel : ViewModelBase
    {
        public DbViewModel()
        {
            SetCommand();
        }

        private void SetCommand()
        {
            ReloadDatabase = new RelayCommand(async () => await Reload());
            ChangeSystemTables = new RelayCommand<bool>(async (b) =>
            {
                if (b)
                {
                    await LoadSystemTables(DbInfo);
                }
                else
                {
                    foreach (var table in Tables.Where(x => (x is TableViewModel) && ((TableViewModel)x).IsSystemTable).ToArray())
                    {
                        Tables.Remove(table);
                    }
                    CollectionViewSource.GetDefaultView(Tables).Refresh();
                    RaisePropertyChanged(nameof(Tables));
                }
            });
        }

        private DatabaseInfo _dbInfo;
        public DatabaseInfo DbInfo { get { return _dbInfo; } }

        public string DisplayDbName { get { return _dbInfo.Path.Substring(_dbInfo.Path.LastIndexOf('\\') + 1); } }

        public List<ITableViewModel> Tables { get; private set; } = new List<ITableViewModel>()
        {
            //暫定
            new TableViewModel("Loading")
        };
        private List<TriggerViewModel> _triggers;
        public List<TriggerViewModel> Triggers
        {
            get
            {
                if (_triggers == null) _triggers = GetTriggers();
                return _triggers;
            }
        }
        private List<TriggerViewModel> GetTriggers()
        {
            return Tables.Where(x => x is TableViewModel).SelectMany(x => ((TableViewModel)x).Triggers).ToList();
        }

        private List<IndexViewModel> _indexes;
        public List<IndexViewModel> Indexes
        {
            get
            {
                if (_indexes == null) _indexes = GetIndexes();
                return _indexes;
            }
        }
        private List<IndexViewModel> GetIndexes()
        {
            return Tables.Where(x => x is TableViewModel).SelectMany(x => ((TableViewModel)x).Indexs).ToList();
        }

        private List<DomainViewModel> _domains;
        public List<DomainViewModel> Domains
        {
            get
            {
                if (_domains == null) _domains = GetDomains();
                return _domains;
            }
        }

        private List<DomainViewModel> GetDomains()
        {
            var list = new List<DomainViewModel>();
            using (var con = new FbConnection(_dbInfo.ConnectionString))
            {
                con.Open();
                foreach (var item in _dbInfo.GetDomain(con))
                {
                    list.Add(new DomainViewModel(item));
                }
            }
            return list;
        }
        private List<ProcedureViewModel> _procedures;
        public List<ProcedureViewModel> Procedures
        {
            get
            {
                if (_procedures == null) _procedures = GetProcedures();
                return _procedures;
            }
        }

        private List<ProcedureViewModel> GetProcedures()
        {
            var list = new List<ProcedureViewModel>();
            using (var con = new FbConnection(_dbInfo.ConnectionString))
            {
                con.Open();
                foreach (var item in _dbInfo.GetProcedures(con))
                {
                    list.Add(new ProcedureViewModel(item));
                }
            }
            return list;
        }
        private List<SequenceViewModel> _sequences;
        public List<SequenceViewModel> Sequences
        {
            get
            {
                if (_sequences == null) _sequences = GetSequences();
                return _sequences;
            }
        }

        private List<SequenceViewModel> GetSequences()
        {
            var list = new List<SequenceViewModel>();
            using (var con = new FbConnection(_dbInfo.ConnectionString))
            {
                con.Open();
                foreach (var item in _dbInfo.GetSequences(con))
                {
                    list.Add(new SequenceViewModel(item));
                }
            }
            return list;
        }

        public string ConnectionString { get { return _dbInfo.ConnectionString; } }
        public string Path { get { return _dbInfo.Path; } }

        public AdditionalDbInfoControl AdditionalInfo { get; private set; }

        public bool CanExecute()
        {
            return !string.IsNullOrEmpty(_dbInfo.ConnectionString);
        }

        public async Task CreateDatabase(string path, FirebirdType type, FbCharset charset)
        {
            var dbInfo = new DatabaseInfo(new FirebirdInfo(path, type, charset));
            dbInfo.CreateDatabase();
            await LoadDatabase(dbInfo);
        }

        public async Task LoadDatabase(DatabaseInfo dbInf)
        {
            _dbInfo = dbInf;
            var table = new List<ITableViewModel>();
            await Task.Run(() =>
             {
                 using (var con = new FbConnection(_dbInfo.ConnectionString))
                 {
                     con.Open();
                     foreach (var item in _dbInfo.GetTables(con))
                     {
                         var vm = new TableViewModel(item.TableName);
                         foreach (var colums in item.GetColums(con))
                         {
                             vm.Colums.Add(new ColumViewMoodel(colums));
                         }
                         foreach (var trigger in item.GetTrigger(con))
                         {
                             vm.Triggers.Add(new TriggerViewModel(trigger));
                         }
                         foreach (var idx in item.GetIndex(con))
                         {
                             vm.Indexs.Add(new IndexViewModel(idx));
                         }
                         table.Add(vm);
                     }

                     foreach (var item in _dbInfo.GetViews(con))
                     {
                         var vm = new TableViewViewModel(item.ViewName, item.Source);
                         foreach (var colums in item.GetColums(con))
                         {
                             vm.Colums.Add(new ColumViewMoodel(colums));
                         }
                         table.Add(vm);
                     }
                 }
             });
            Tables = table;
            AdditionalInfo = new AdditionalDbInfoControl(this);
            RaisePropertyChanged(nameof(Tables));
            RaisePropertyChanged(nameof(AdditionalInfo));
        }

        public async Task LoadSystemTables(DatabaseInfo dbInf)
        {
            _dbInfo = dbInf;
            var table = new List<ITableViewModel>();
            await Task.Run(() =>
            {
                using (var con = new FbConnection(_dbInfo.ConnectionString))
                {
                    con.Open();
                    foreach (var item in _dbInfo.GetSystemTables(con))
                    {
                        var vm = new TableViewModel(item.TableName, true);
                        foreach (var colums in item.GetColums(con))
                        {
                            vm.Colums.Add(new ColumViewMoodel(colums));
                        }
                        table.Add(vm);
                    }
                }
            });
            Tables.AddRange(table);
            CollectionViewSource.GetDefaultView(Tables).Refresh();
            RaisePropertyChanged(nameof(Tables));
        }

        public ICommand ReloadDatabase { get; private set; }
        public async Task Reload()
        {
            Tables.Clear();
            _triggers = null;
            _indexes = null;
            _domains = null;
            await LoadDatabase(new DatabaseInfo(new FirebirdInfo(_dbInfo.Path)));
        }
        public bool IsSystemTableChecked { get; set; }
        public ICommand ChangeSystemTables { get; private set; }
    }
}