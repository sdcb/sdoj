using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace SdojJudger.SandboxDll
{
    public class Sandbox
    {
        public static SandboxIoResult BeginRun(SandboxRunInfo info)
        {
            var apiInfo = (ApiRunInfo) info;
            var apiIoResult = new ApiRunIoResult();

            NativeBeginRun(ref apiInfo, ref apiIoResult);

            var result = (SandboxIoResult) apiIoResult;
            return result;
        }

        public static SandboxRunResult EndRun(IntPtr instance)
        {
            var apiResult = new ApiRunResult();

            NativeEndRun(instance, ref apiResult);

            var result = (SandboxRunResult) apiResult;

            return result;
        }





#if DEBUG
        [DllImport(@"Reference\Debug\safe_runner", 
#else 
        [DllImport(@"Reference\Release\safe_runner", 
#endif
            EntryPoint = "begin_run", CharSet = CharSet.Unicode)]
        private static extern void NativeBeginRun(ref ApiRunInfo info, ref ApiRunIoResult result);

#if DEBUG
        [DllImport(@"Reference\Debug\safe_runner", 
#else
        [DllImport(@"Reference\Release\safe_runner",
#endif
            EntryPoint = "end_run", CharSet = CharSet.Unicode)]
        private static extern void NativeEndRun(IntPtr instance, ref ApiRunResult result);

        [StructLayout(LayoutKind.Sequential)]
        private struct ApiRunInfo
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            private string Path;

            private int PathLength;

            private int TimeLimitMs;

            private float MemoryLimitMb;

            private bool LimitProcessCount;

            public static explicit operator ApiRunInfo(SandboxRunInfo info)
            {
                return new ApiRunInfo
                {
                    Path = info.Path, 
                    PathLength = info.Path.Length, 
                    TimeLimitMs = info.TimeLimitMs, 
                    MemoryLimitMb = info.MemoryLimitMb, 
                    LimitProcessCount = info.LimitProcessCount, 
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ApiRunIoResult
        {
            readonly bool Succeed;

            readonly IntPtr InstanceHandle;

            readonly int ErrorCode;

            readonly IntPtr InputWriteHandle;

            readonly IntPtr OutputReadHandle;

            readonly IntPtr ErrorReadHandle;

            public static explicit operator SandboxIoResult(ApiRunIoResult apiResult)
            {
                var result = new SandboxIoResult
                {
                    Succeed = apiResult.Succeed, 
                    ErrorCode = apiResult.ErrorCode, 
                    InstanceHandle = apiResult.InstanceHandle, 
                };

                var win32Exceptio = new Win32Exception(result.ErrorCode);
                result.ErrorMessage = win32Exceptio.Message;

                if (apiResult.InputWriteHandle != IntPtr.Zero &&
                    apiResult.OutputReadHandle != IntPtr.Zero &&
                    apiResult.ErrorReadHandle != IntPtr.Zero)
                {
                    result.InputWriteStream = new FileStream(
                        new SafeFileHandle(apiResult.InputWriteHandle, true),
                        FileAccess.Write);
                    result.OutputReadStream = new FileStream(
                        new SafeFileHandle(apiResult.OutputReadHandle, true),
                        FileAccess.Read);
                    result.ErrorReadStream = new FileStream(
                        new SafeFileHandle(apiResult.ErrorReadHandle, true),
                        FileAccess.Read);
                }

                return result;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ApiRunResult
        {
            readonly bool Succeed;

            readonly int ErrorCode;

            readonly int ExitCode;

            readonly int TimeMs;

            readonly int MemoryMB;

            public static explicit operator SandboxRunResult(ApiRunResult apiResult)
            {
                var result = new SandboxRunResult
                {
                    ErrorCode = apiResult.ErrorCode, 
                    Succeed = apiResult.Succeed, 
                    MemoryMb = apiResult.MemoryMB, 
                    TimeMs = apiResult.TimeMs
                };

                var win32Exception = new Win32Exception(apiResult.ErrorCode);
                result.ErrorMessage = win32Exception.Message;

                return result;
            }
        }
    }

    
}
