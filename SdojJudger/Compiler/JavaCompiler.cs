using System;
using System.Diagnostics;
using System.IO;
using SdojJudger.Compiler.Infrastructure;

namespace SdojJudger.Compiler
{
    public class JavaCompiler : CompilerProvider
    {
        public override CompileResult Compile(string source)
        {
            _filename = GetTempFileNameWithoutExtension();
            File.WriteAllText(_filename + ".java", source);

            string info = CompileSourceFile(_filename);
            info = info?.Replace(_filename, "Source");

            if (File.Exists(_filename + ".class"))
            {
                return new CompileResult
                {
                    HasErrors = false, 
                    Output = info,
                    PathToAssembly = $"{GetJavaPath(AppSettings.JdkBinPath)} -Xms1m -cp {Path.GetDirectoryName(_filename)} Program"
                };
            }
            else
            {
                return new CompileResult
                {
                    HasErrors = true, 
                    Output = info, 
                    PathToAssembly = null
                };
            }
        }

        private string _filename;

        public override void Dispose()
        {
            if (_filename != null)
            {
                Directory.Delete(Path.GetDirectoryName(_filename), true);
            }
        }

        private string CompileSourceFile(string filename)
        {
            var pi = new ProcessStartInfo(GetJavaCPath(AppSettings.JdkBinPath))
            {
                UseShellExecute = false, 
                CreateNoWindow = true, 
                RedirectStandardInput = true, 
                RedirectStandardOutput = true, 
                RedirectStandardError = true, 
                Arguments = filename + ".java"
            };
            var ps = Process.Start(pi);

            var output = ps.StandardOutput.ReadToEnd();
            var error = ps.StandardError.ReadToEnd();

            return output + error;
        }

        private string GetTempFileNameWithoutExtension()
        {
            var folder = Path.Combine(Path.GetTempPath(), "judge", Guid.NewGuid().ToString());
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, "Program");
        }

        private static string GetJavaCPath(string jdkBinPath)
        {
            return Path.Combine(jdkBinPath, "javac.exe");
        }

        private static string GetJavaPath(string jdkBinPath)
        {
            return Path.Combine(jdkBinPath, "java.exe");
        }
    }
}
