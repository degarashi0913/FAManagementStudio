using FAManagementStudio.Common;
using FAManagementStudio.Models.Firebird;
using FirebirdSql.Data.FirebirdClient;

namespace FAManagementStudio.Models;

public class FirebirdInfo : BindableBase
{
    // TODO: 後で削除する
    public FirebirdInfo() { }

    public FirebirdInfo(string path)
    {
        SetConnection(path);
    }

    public FirebirdInfo(string path, FirebirdType fbType, FbCharset charset)
    {
        Builder.Database = path;
        Builder.ClientLibrary = fbType == FirebirdType.Fb3 ? @"fb3\fbclient" : System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Environment.ProcessPath) ?? string.Empty, @"fb25\fbembed.dll");
        Builder.Charset = new FbUtility().GetCharsetString(charset);
    }

    public string Path { get { return Builder.Database; } }

    private const int _fb2Ods = 11;
    private const int _fb3Ods = 12;
    private int _odsVersion = -1;

    private void SetConnection(string path)
    {
        _odsVersion = new FbUtility().GetOdsVersion(path);
        Builder.Database = path;
        Builder.ClientLibrary = _odsVersion == _fb3Ods ? @"fb3\fbclient" : System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Environment.ProcessPath) ?? string.Empty, @"fb25\fbembed.dll");
    }

    public bool IsTargetOdsVersion() => _odsVersion == _fb2Ods || _odsVersion == _fb3Ods;

    public FbConnectionStringBuilder Builder { get; }
        = new FbConnectionStringBuilder()
        {
            DataSource = "localhost",
            Charset = "UTF8",
            UserID = "SYSDBA",
            Password = "masterkey",
            ServerType = FbServerType.Embedded,
            Pooling = false
        };
}
