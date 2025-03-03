using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace HamsterStudio.Toolkits
{
    public static class HashUtils
    {
        public static byte[] ToByteArray<T>(this T data)
        {
            byte[] buffer = new byte[Marshal.SizeOf(data)];
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            Marshal.Copy(handle.AddrOfPinnedObject(), buffer, 0, buffer.Length);
            handle.Free();
            return buffer;
        }

        public static string GetHexString(this byte[] data)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.Append(data[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static byte[] GetMd5(this byte[] data)
        {
            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(data);
            return hashBytes;
        }
    }
}
