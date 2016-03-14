using FAManagementStudio.Common;

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
        }
        public static QueryTabViewModel GetNewInstance()
        {
            return new QueryTabViewModel("+", "");
        }
    }
}

