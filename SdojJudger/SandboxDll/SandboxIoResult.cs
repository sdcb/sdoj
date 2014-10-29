using System;
using System.IO;

namespace SdojJudger.SandboxDll
{
    public class SandboxIoResult
    {
        public bool Succeed { get; set; }

        public int ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public IntPtr InstanceHandle { get; set; }

        public FileStream InputWriteStream { get; set; }

        public FileStream OutputReadStream { get; set; }

        public FileStream ErrorReadStream { get; set; }
    }
}
