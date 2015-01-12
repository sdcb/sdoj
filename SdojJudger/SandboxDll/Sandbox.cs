using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using SdojJudger.Runner;

namespace SdojJudger.SandboxDll
{
    public class Sandbox
    {
        public static SandboxIoResult BeginRun(SandboxRunInfo info)
        {
            var apiInfo = (ApiRunInfo)info;
            var apiIoResult = new ApiRunIoResult();

            NativeBeginRun(ref apiInfo, ref apiIoResult);

            var result = (SandboxIoResult)apiIoResult;
            return result;
        }

        public static SandboxRunResult EndRun(IntPtr instance)
        {
            var apiResult = new ApiRunResult();

            NativeEndRun(instance, ref apiResult);

            var result = (SandboxRunResult)apiResult;

            return result;
        }

        public static JudgeResult Judge(JudgeInfo ji)
        {
            var info = new SandboxRunInfo
            {
                LimitProcessCount = 1,
                MemoryLimitMb = ji.MemoryLimitMb,
                Path = ji.Path,
                TimeLimitMs = ji.TimeLimitMs
            };

            SandboxIoResult ior = BeginRun(info);

            if (!ior.Succeed || ior.ErrorCode != 0)
            {
                EndRun(ior.InstanceHandle);

                return new JudgeResult
                {
                    ErrorCode = ior.ErrorCode,
                    ErrorMessage = ior.ErrorMessage,
                };
            }
            ior.ErrorReadStream.Dispose();
            var writeTask = Task.Run(async () =>
            {
                using (var writer = new StreamWriter(ior.InputWriteStream))
                {
                    await writer.WriteAsync(ji.Input);
                }
            });
            var readTask = Task.Run(async () =>
            {
                var buffer = new char[4096];
                var result = new StringBuilder(4096);
                using (var reader = new StreamReader(ior.OutputReadStream, Encoding.Default))
                {
                    while (true)
                    {
                        var c = await reader.ReadAsync(buffer, 0, buffer.Length);
                        if (c > 0)
                        {
                            result.Append(buffer, 0, c);
                            if (result.Length > LimitedSize)
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                return result.ToString();
            });

            SandboxRunResult res = EndRun(ior.InstanceHandle);

            writeTask.Wait();
            readTask.Wait();

            var jr = new JudgeResult
            {
                ErrorCode = res.ErrorCode,
                ErrorMessage = res.ErrorMessage,
                ExceptionCode = 0,
                ExceptionMessage = null,
                ExitCode = res.ExitCode,
                MemoryMb = res.MemoryMb,
                Output = readTask.Result,
                Succeed = res.Succeed,
                TimeMs = res.TimeMs,
            };
            return jr;
        }

        private const int LimitedSize = 20 * 1024 * 1024;


#if DEBUG
        [DllImport(@"Reference\Debug\sandbox",
#else 
        [DllImport(@"Reference\Release\sandbox", 
#endif
 EntryPoint = "begin_run", CharSet = CharSet.Unicode)]
        private static extern void NativeBeginRun(ref ApiRunInfo info, ref ApiRunIoResult result);

#if DEBUG
        [DllImport(@"Reference\Debug\sandbox",
#else
        [DllImport(@"Reference\Release\sandbox",
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

            private int LimitProcessCount;

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

            readonly float MemoryMB;

            public static explicit operator SandboxRunResult(ApiRunResult apiResult)
            {
                var result = new SandboxRunResult
                {
                    ErrorCode = apiResult.ErrorCode,
                    Succeed = apiResult.Succeed,
                    MemoryMb = apiResult.MemoryMB,
                    TimeMs = apiResult.TimeMs,
                    ExitCode = apiResult.ExitCode
                };

                var win32Exception = new Win32Exception(apiResult.ErrorCode);
                result.ErrorMessage = win32Exception.Message;

                return result;
            }
        }
    }


}
