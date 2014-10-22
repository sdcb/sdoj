using System;
using System.Diagnostics;
using System.IO;
using SdojJudger.Compiler.Infrastructure;

namespace SdojJudger.Compiler
{
    public class Python3Compiler : CompilerProvider
    {
        public override CompileResult Compile(string source)
        {
            var filename = GetTempFileNameWithoutExtension();
            File.WriteAllText(filename + ".py", source);

            var info = CompileSourceFile(filename);
            info = info.Replace(filename, "Source");

            if (File.Exists(filename + ".pyc"))
            {
                return new CompileResult
                {
                    HasErrors = false, 
                    Output = info, 
                    PathToAssembly = string.Format("{0} {1}.pyc", AppSettings.Python3Path, filename)
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

        private string CompileSourceFile(string filename)
        {
            var pi = new ProcessStartInfo(AppSettings.Python3Path)
            {
                UseShellExecute = false, 
                CreateNoWindow = true, 
                RedirectStandardInput = true, 
                RedirectStandardOutput = true, 
                RedirectStandardError = true, 
                Arguments = string.Format(
                    "-c \"import py_compile; py_compile.compile(r'{0}.py', r'{0}.pyc') \"", 
                    filename)
            };
            var ps = Process.Start(pi);

            var output = ps.StandardOutput.ReadToEnd();
            var error = ps.StandardError.ReadToEnd();

            return output + error;
        }

        private string GetTempFileNameWithoutExtension()
        {
            var filename = Path.Combine(
                Path.GetTempPath(),
                "judge-" + Guid.NewGuid());
            return filename;
        }
    }
}
