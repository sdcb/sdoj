using System.Runtime.InteropServices;
using System.Text;

namespace SafeRunnerTest
{
    public static class NativeDll
    {
        [DllImport("safe_runner", EntryPoint = "cluck")]
        public extern static void Cluck();

        [DllImport("safe_runner", EntryPoint = "get3")]
        public static extern int Get3();

        [DllImport("safe_runner", EntryPoint = "string_length", CallingConvention = CallingConvention.Cdecl, CharSet =  CharSet.Unicode  )]
        public static extern int StringLength(string str);

        [DllImport("safe_runner", EntryPoint = "concat_string_table", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern bool ConcatStringTable(
            ref StringTable table,
            StringBuilder result, 
            int length);

        [StructLayout(LayoutKind.Sequential)]
        public struct StringTable
        {
            public string String1;
            public int Length1;
            public string String2;
            public int Length2;
            public string String3;
            public int Length3;
        }

        [DllImport("safe_runner", EntryPoint = "concat_string_args", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern bool ConcatStringArgs(
            string s1, int length1,
            string s2, int length2,
            string s3, int length3,
            StringBuilder buffer, int length);
    }
}
