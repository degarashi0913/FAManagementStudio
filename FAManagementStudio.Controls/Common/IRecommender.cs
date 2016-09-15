using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FAManagementStudio.Controls.Common
{
    public interface IRecommender
    {
        Task<List<CompletionData>> GetCompletionData(string inputString, int index);
    }
}
