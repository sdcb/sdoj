using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SafeRunnerTest
{
    public static class NativeDll
    {
        [DllImport("safe_runner", 
            EntryPoint = "judge", 
            CallingConvention = CallingConvention.Cdecl, 
            SetLastError = true, 
            CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Judge(ref ApiJudgeInfo info, ref ApiJudgeResult result);

        [DllImport("safe_runner", EntryPoint = "free_judge_result", CallingConvention = CallingConvention.Cdecl, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern void FreeJudgeResult(ref ApiJudgeResult result);

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

        [DllImport("safe_runner", EntryPoint = "concat_string_args", CallingConvention = CallingConvention.Cdecl,
            CharSet = CharSet.Unicode)]
        public static extern bool ConcatStringArgs(
            string s1, int length1,
            string s2, int length2,
            string s3, int length3,
            StringBuilder buffer, int length);

        [StructLayout(LayoutKind.Sequential)]
        public struct StringTable
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string String1;
            public int Length1;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string String2;
            public int Length2;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string String3;
            public int Length3;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ApiJudgeInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Path;
            public int PathLength;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Input;
            public int InputLength;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Output;
            public int OutputLength;
            public int TimeLimitMs;
            public float MemoryLimitMb;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ApiJudgeResult
        {
            public uint ErrorCode;
            public uint ExceptionCode;
            public int Time;
            public float Memory;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Output;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ErrorMessage;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ExceptionMessage;
        };
    }
}
