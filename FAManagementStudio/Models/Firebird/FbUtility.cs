using System.IO;
using System.Runtime.InteropServices;

namespace FAManagementStudio.Models.Firebird
{
    public class FbUtility
    {
        private const ushort FirebirdFlag = 0x8000;

        private T ConvertFromBinary<T>(byte[] rawData) where T : struct
        {
            var pinnedData = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(pinnedData.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                pinnedData.Free();
            }
        }

        public int GetOdsVersion(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                var buf = new byte[1024];
                stream.Read(buf, 0, buf.Length);
                var header = ConvertFromBinary<HeaderPage>(buf);
                return header.hdr_ods_version & ~FirebirdFlag;
            }
        }
    }
}
