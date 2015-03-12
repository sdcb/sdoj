using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SdojJudger.Runner
{
    public static class NativeDll
    {
        // The only public operator.
        public static JudgeResult Judge(JudgeInfo info)
        {
            var aji = (ApiJudgeInfo)(info);
            var ajr = new ApiJudgeResult();

            try
            {
                var ok = Judge(ref aji, ref ajr);
                var result = (JudgeResult)ajr;
                result.IsDone = ok;
                return result;
            }
            finally
            {
                FreeJudgeResult(ref ajr);
            }
        }

        // P/Invoke functions
#if DEBUG
        [DllImport(@"Reference\Debug\sandbox", 
#else
        [DllImport(@"Reference\Release\sandbox", 
#endif
            EntryPoint = "free_judge_result", 
            CallingConvention = CallingConvention.Cdecl, 
            SetLastError = true, 
            CharSet = CharSet.Unicode)]
        private static extern void FreeJudgeResult(ref ApiJudgeResult result);

#if DEBUG
        [DllImport(@"Reference\Debug\sandbox",
#else
        [DllImport(@"Reference\Release\sandbox",
#endif
            EntryPoint = "judge",
            CallingConvention = CallingConvention.Cdecl,
            SetLastError = true,
            CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Judge(ref ApiJudgeInfo info, ref ApiJudgeResult result);

        // P/Invoke Structures
        [StructLayout(LayoutKind.Sequential)]
        private struct ApiJudgeInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Path;

            public int PathLength;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Input;

            public int InputLength;

            public int TimeLimitMs;

            public float MemoryLimitMb;

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
        private struct ApiJudgeResult
        {
            public readonly int ErrorCode;
            public readonly int ExceptionCode;
            public readonly int ExitCode;
            public readonly int TimeMs;
            public readonly float MemoryMb;
            public IntPtr Output;
            public IntPtr ExceptionMessage;

            public static explicit operator JudgeResult(NativeDll.ApiJudgeResult info)
            {
                var result = new JudgeResult
                {
                    ErrorCode = info.ErrorCode,
                    //ExceptionCode = info.ExceptionCode,
                    TimeMs = info.TimeMs,
                    MemoryMb = info.MemoryMb,
                };

                if (info.Output != IntPtr.Zero)
                {
                    result.Output = Marshal.PtrToStringUni(info.Output);
                }

                if (info.ExceptionMessage != IntPtr.Zero)
                {
                    //result.ExceptionMessage = Marshal.PtrToStringUni(info.ExceptionMessage);
                }

                var win32Exception = new Win32Exception(info.ErrorCode);
                result.ErrorMessage = win32Exception.Message;

                result.ExitCode = info.ExitCode;

                return result;
            }
        };
    }
}
