using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
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

        public static JudgeResult Judge(JudgeInfo info)
        {
            var aji = (ApiJudgeInfo) (info);
            var ajr = new ApiJudgeResult();

            try
            {
                var ok = Judge(ref aji, ref ajr);
                var result = (JudgeResult) ajr;
                result.Succeed = ok;
                return result;
            }
            finally
            {
                FreeJudgeResult(ref ajr);
            }
        }

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

        public int TimeLimitMs;

        public float MemoryLimitMb;
    }

    public class JudgeInfo
    {
        public string Path { get; set; }

        public string Input { get; set; }

        public int TimeLimitMs { get; set; }

        public float MemoryLimitMb { get; set; }


        public static explicit operator ApiJudgeInfo(JudgeInfo info)
        {
            // info.Path must not be null.

            var aji = new ApiJudgeInfo
            {
                Path = info.Path, 
                PathLength = info.Path.Length, 
                Input = info.Input, 
                TimeLimitMs = info.TimeLimitMs, 
                MemoryLimitMb = info.MemoryLimitMb
            };

            if (info.Input != null)
            {
                aji.InputLength = info.Input.Length;
            }

            return aji;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ApiJudgeResult
    {
        public readonly int ErrorCode;
        public readonly int ExceptionCode;
        public readonly int TimeMs;
        public readonly float MemoryMb;
        public IntPtr Output;
        public IntPtr ExceptionMessage;
    };

    public class JudgeResult
    {
        public int ErrorCode { get; set; }

        public int ExceptionCode { get; set; }

        public int TimeMs { get; set; }

        public float MemoryMb { get; set; }

        public string Output { get; set; }

        public string ErrorMessage { get; set; }

        public string ExceptionMessage { get; set; }

        public bool Succeed { get; set; }

        public static explicit operator JudgeResult(ApiJudgeResult info)
        {
            var result = new JudgeResult
            {
                ErrorCode = info.ErrorCode, 
                ExceptionCode = info.ExceptionCode, 
                TimeMs = info.TimeMs, 
                MemoryMb = info.MemoryMb, 
            };

            if (info.Output != IntPtr.Zero)
            {
                result.Output = Marshal.PtrToStringUni(info.Output);
            }

            if (info.ExceptionMessage != IntPtr.Zero)
            {
                result.ExceptionMessage = Marshal.PtrToStringUni(info.ExceptionMessage);
            }

            var win32Exception = new Win32Exception(info.ErrorCode);
            result.ErrorMessage = win32Exception.Message;

            return result;
        }
    }
}
