using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Snappy
{
    abstract class NativeProxy
    {
        protected NativeProxy(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var folder = Path.Combine(Path.GetTempPath(), "Snappy.NET-" + assembly.GetName().Version.ToString());
            Directory.CreateDirectory(folder);
            var path = Path.Combine(folder, name);
            byte[] contents;
            using (var input = assembly.GetManifestResourceStream(name))
            using (var buffer = new MemoryStream())
            {
                input.CopyTo(buffer);
                buffer.Close();
                contents = buffer.ToArray();
            }
            if (!File.Exists(path) || !File.ReadAllBytes(path).SequenceEqual(contents))
            {
                using (var output = File.Open(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    output.Write(contents, 0, contents.Length);
            }
            var h = LoadLibrary(path);
            if (h == IntPtr.Zero)
                throw new ApplicationException("Cannot load " + name);
        }

        public unsafe abstract SnappyStatus Compress(byte* input, int inLength, byte* output, ref int outLength);
        public unsafe abstract SnappyStatus Uncompress(byte* input, int inLength, byte* output, ref int outLength);
        public abstract int GetMaxCompressedLength(int inLength);
        public unsafe abstract SnappyStatus GetUncompressedLength(byte* input, int inLength, out int outLength);
        public unsafe abstract SnappyStatus ValidateCompressedBuffer(byte* input, int inLength);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr LoadLibrary(string lpFileName);
    }
}
