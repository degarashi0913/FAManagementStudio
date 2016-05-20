using FAManagementStudio.Models.Firebird;

namespace FAManagementStudio.Models
{
    public class FirebirdInfo
    {
        private const int _fb2Ods = 11;
        public bool IsTargetOdsDb(string path)
        {
            var inf = new FbUtility();
            try
            {
                return inf.GetOdsVersion(path) == _fb2Ods;
            }
            catch
            {
                return false;
            }
        }
    }
}
