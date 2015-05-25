using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using SdojJudger.Runner;
using System.Threading;
using System.Diagnostics;

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

        public static Process2JudgeResult Process2Judge(Process2JudgeInfo p2info)
        {
            // 步骤：
            // 1、启动评测
            // 2、将进程输入、输出重定向
            // 3、等待程序运行完成
            // 4、处理并返回程序运行结果。

            // 1、启动评测
            var info1 = new SandboxRunInfo
            {
                LimitProcessCount = 1,
                MemoryLimitMb = 512.0f,
                TimeLimitMs = 10000,
                Path = p2info.Path1
            };
            SandboxIoResult io1 = BeginRun(info1);
            if (!io1.Succeed)
            {
                EndRun(io1.InstanceHandle);
                return new Process2JudgeResult
                {
                    P1Result = (JudgeResult)io1
                };
            }

            var info2 = new SandboxRunInfo
            {
                LimitProcessCount = 1,
                MemoryLimitMb = p2info.MemoryLimitMb,
                TimeLimitMs = p2info.TimeLimitMs,
                Path = p2info.Path2
            };
            SandboxIoResult io2 = BeginRun(info2);
            if (!io2.Succeed)
            {
                EndRun(io2.InstanceHandle);
                return new Process2JudgeResult
                {
                    P2Result = (JudgeResult)io2
                };
            }

            // 2、将两个进程的输入输出重定向。
            Func<FileStream, FileStream, CancellationToken, Task> transformFunc = async (toRead, toWrite, cancel) =>
            {
                var buffer = new char[4096];
                using (var reader = new StreamReader(toRead, Encoding.Default))
                using (var writer = new StreamWriter(toWrite))
                {
                    while (!cancel.IsCancellationRequested)
                    {
                        var c = await reader.ReadAsync(buffer, 0, buffer.Length);

                        if (c != 0)
                        {
                            await writer.WriteAsync(buffer, 0, c);
                            Debug.WriteLine($"{Task.CurrentId}: {new string(buffer, 0, c)}");
                            await writer.FlushAsync();
                        }
                        else
                        {
                            await Task.Delay(10);
                        }
                    }
                }
            };

            var cancelToken = new CancellationTokenSource();
            var transform12 = transformFunc(io1.OutputReadStream, io2.InputWriteStream, cancelToken.Token);
            var transform21 = transformFunc(io2.OutputReadStream, io1.InputWriteStream, cancelToken.Token);
            var stdErrorTask = ReadToEndFrom(io1.ErrorReadStream);
            io2.ErrorReadStream.Dispose();

            // 3、等待程序运行完成
            var run1 = EndRun(io1.InstanceHandle);
            var run2 = EndRun(io2.InstanceHandle);
            cancelToken.Cancel(throwOnFirstException: false);
            transform21.Wait();
            transform12.Wait();
            stdErrorTask.Wait();

            // 4、返回运行结果。
            var toReturn = new Process2JudgeResult
            {
                P1Result = (JudgeResult)run1,
                P2Result = (JudgeResult)run2
            };
            toReturn.P1Result.Output = stdErrorTask.Result;
            return toReturn;
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

            if (!ior.Succeed)
            {
                EndRun(ior.InstanceHandle);

                return (JudgeResult)ior;
            }
            ior.ErrorReadStream.Dispose();
            var writeTask = Task.Run(async () =>
            {
                using (var writer = new StreamWriter(ior.InputWriteStream, Encoding.Default))
                {
                    await writer.WriteAsync(ji.Input);
                }
            });
            var readTask = ReadToEndFrom(ior.OutputReadStream);

            SandboxRunResult res = EndRun(ior.InstanceHandle);

            writeTask.Wait();
            readTask.Wait();

            var jr = (JudgeResult)res;
            jr.Output = readTask.Result;

            return jr;
        }

        private static async Task<string> ReadToEndFrom(FileStream stream)
        {
            var buffer = new char[4096];
            var result = new StringBuilder(4096);
            using (var reader = new StreamReader(stream, Encoding.Default))
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
                    IsDone = apiResult.Succeed,
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
