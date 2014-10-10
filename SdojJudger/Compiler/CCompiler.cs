using System;
using System.Diagnostics;
using System.IO;

namespace SdojJudger.Compiler
{
    public class CCompiler : CompilerProvider
    {
        public override CompileResult Compile(string source)
        {
            var filename = GetTempFileNameWithoutExtension();

            File.WriteAllText(filename + ".c", source);

            CompileCppFile(filename);

            var executableFile = filename + ".exe";
            var log = File.ReadAllText(filename + ".txt");
            if (!File.Exists(executableFile))
            {
                return new CompileResult { HasErrors = true, Output = log, PathToAssembly = null };
            }

            return new CompileResult
            {
                HasErrors = false, 
                Output = log, 
                PathToAssembly = executableFile
            };
        }

        private static string GetTempFileNameWithoutExtension()
        {
            var filename = Path.Combine(
                Path.GetTempPath(),
                "judge-" + Guid.NewGuid());
            return filename;
        }

        private static void CompileCppFile(string sourceFile)
        {
            var arg = "/Q /K " + "\"" + AppSettings.VcCommandline + "\"";
            var cl = string.Format("cl \"{0}.c\" /Fe:\"{0}.exe\" /nologo /Ox > \"{0}.txt\"", sourceFile) + 
                     Environment.NewLine + 
                     "exit";
            var info = new ProcessStartInfo("cmd.exe")
            {
                Arguments = arg, 
                UseShellExecute = false, 
                CreateNoWindow = true, 
                RedirectStandardInput = true, 
            };
            var ps = Process.Start(info);

            ps.StandardInput.WriteLine(cl);
            ps.WaitForExit();
        }
    }
}