using FirebirdSql.Data.FirebirdClient;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace FAManagementStudio.Models.Firebird;

public class FbUtility
{
    private const ushort FirebirdFlag = 0x8000;

    private T ConvertFromBinary<T>(byte[] rawData) where T : struct
    {
        var pinnedData = GCHandle.Alloc(rawData, GCHandleType.Pinned);
        try
        {
            return Marshal.PtrToStructure<T>(pinnedData.AddrOfPinnedObject());
        }
        finally
        {
            pinnedData.Free();
        }
    }

    public int GetOdsVersion(string path)
    {
        using FileStream stream = new(path, FileMode.Open);
        var buf = new byte[1024];
        stream.ReadExactly(buf);
        var header = ConvertFromBinary<HeaderPage>(buf);
        return header.hdr_ods_version & ~FirebirdFlag;
    }

    public string GetCharsetString(FbCharset charset)
    {
        var dic = new Dictionary<FbCharset, string>
            {
                {FbCharset.Octets, "OCTETS" },
                {FbCharset.Ascii, "ASCII"},
                {FbCharset.UnicodeFss, "UNICODE_FSS"},
                {FbCharset.Utf8, "UTF8"},
                {FbCharset.ShiftJis0208, "SJIS_0208"},
                {FbCharset.EucJapanese0208, "EUCJ_0208"},
                {FbCharset.Iso2022Japanese, "ISO2022-JP"},
                {FbCharset.Dos437, "DOS437"},
                {FbCharset.Dos850, "DOS850"},
                {FbCharset.Dos865, "DOS865"},
                {FbCharset.Dos860, "DOS860"},
                {FbCharset.Dos863, "DOS863"},
                {FbCharset.Iso8859_1, "ISO8859_1"},
                {FbCharset.Iso8859_2, "ISO8859_2"},
                {FbCharset.Ksc5601, "KSC_5601"},
                {FbCharset.Dos861, "DOS861"},
                {FbCharset.Windows1250, "WIN1250"},
                {FbCharset.Windows1251, "WIN1251"},
                {FbCharset.Windows1252, "WIN1252"},
                {FbCharset.Windows1253, "WIN1253"},
                {FbCharset.Windows1254, "WIN1254"},
                {FbCharset.Big5, "BIG_5"},
                {FbCharset.Gb2312, "GB_2312"},
                {FbCharset.Windows1255, "WIN1255"},
                {FbCharset.Windows1256, "WIN1256"},
                {FbCharset.Windows1257, "WIN1257"},
                {FbCharset.Koi8R, "KOI8R"},
                {FbCharset.Koi8U, "KOI8U"},
                {FbCharset.None, "NONE" },
                {FbCharset.Default,""}
            };
        return dic[charset];
    }
}
