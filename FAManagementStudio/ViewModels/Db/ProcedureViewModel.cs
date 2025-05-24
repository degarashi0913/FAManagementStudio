using FAManagementStudio.Common;
using FAManagementStudio.Models;

namespace FAManagementStudio.ViewModels
{
    public class ProcedureViewModel : BindableBase
    {
        private ProcedureInfo _inf;
        public ProcedureViewModel(ProcedureInfo inf)
        {
            _inf = inf;
        }
        public string ProcedureName { get { return _inf.ProcedureName; } }
        public string Source { get { return _inf.Source; } }
    }
}