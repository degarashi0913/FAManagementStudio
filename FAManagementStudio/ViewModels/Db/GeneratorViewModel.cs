using FAManagementStudio.Common;
using FAManagementStudio.Models;

namespace FAManagementStudio.ViewModels
{
    public class GeneratorViewModel : BindableBase, IAddtionalDbInfo
    {
        private GeneratorInfo _inf;
        public GeneratorViewModel(GeneratorInfo inf)
        {
            _inf = inf;
        }
        public string Name { get { return _inf.Name; } }
        public long CurrentValue { get { return _inf.CurrentValue; } }
    }
}
