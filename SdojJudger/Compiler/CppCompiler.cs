using System;
using System.CodeDom.Compiler;
using System.IO;
using SdojJudger.Runner;

namespace SdojJudger.Compiler
{
    public class CppCompiler : CompilerProvider
    {
        public override CompileResult Compile(string sourceCode)
        {
            var sourceFile = GetTempSourceFileName() + ".cpp";
            var exeFile = sourceFile + ".exe";

            File.WriteAllText(sourceFile, sourceCode);

            var path = string.Format(@"cmd /K {0}", AppSettings.VcCommandline);
            var commandInput = 
                "echo off \r\n" +
                "cl {1} /nologo /Ox", 
                Environment.NewLine, 
                "exit");

            var result = ProcessStart(commandInput);

            
        }

        public static JudgeResult ProcessStart(string cmd)
        {
            var info = new JudgeInfo
            {
                Input = cmd, 
                Path = AppSettings.VcCommandline, 
                MemoryLimitMb = 1024.0f, 
                TimeLimitMs = 10000
            };

            var result = NativeDll.Judge(info);

            return result;
        }

        public static string GetTempSourceFileName()
        {
            var tempDir = Path.GetTempPath();
            var filename = string.Format("judge-{0}", Guid.NewGuid());
            var fullpath = Path.Combine(tempDir, filename);
            return fullpath;
        }
    }
}