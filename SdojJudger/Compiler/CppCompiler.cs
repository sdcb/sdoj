using System;
using System.Diagnostics;
using System.IO;

namespace SdojJudger.Compiler
{
    public class CppCompiler : CompilerProvider
    {
        public override CompileResult Compile(string source)
        {
            var filename = GetTempFileNameWithoutExtension();

            File.WriteAllText(filename + ".cpp", source);

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
            if (AppSettings.GccPath != null)
            {
                CompileByGcc(sourceFile);
                return;
            }
            //if (AppSettings.VcCommandline != null)
            //{
            //    CompileByVc(sourceFile);
            //    return;
            //}
        }

        private static void CompileByGcc(string sourceFile)
        {
            // C11
            // gcc -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s 
            // -std=c11 -Wl,--stack=67108864 -O2 -o %1.exe %1

            // C++11
            // g++ -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s 
            // -x c++ -std=c++11 -Wl,--stack=67108864 -O2 -o %1.exe %1

            var pi = new ProcessStartInfo("cmd.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true, 
                RedirectStandardInput = true, 
                Arguments = "/Q"
            };

            var gcc = Path.Combine(AppSettings.GccPath, "g++.exe");
            var input =
                "setlocal" + Environment.NewLine +
                "set path=%path%;" + AppSettings.GccPath + Environment.NewLine +
                string.Format(
                    "{0} -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s -x c++ -std=c++11 -O2 -o {1}.exe {1}.cpp" +
                    " > {1}.txt 2>&1", 
                    gcc, 
                    sourceFile) + Environment.NewLine +
                "exit";

            var ps = Process.Start(pi);
            ps.StandardInput.WriteLine(input);
            ps.StandardInput.Close();

            ps.WaitForExit();
        }

        private static void CompileByVc(string sourceFile)
        {
            var arg = "/Q /K " + "\"" + AppSettings.VcCommandline + "\"";
            var cl = string.Format("cl \"{0}.cpp\" /Fe:\"{0}.exe\" /nologo /Ox > \"{0}.txt\"", sourceFile) +
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