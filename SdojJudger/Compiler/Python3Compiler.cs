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
            _filename = GetTempFileNameWithoutExtension();
            File.WriteAllText(_filename + ".py", source);

            var info = CompileSourceFile(_filename);
            info = info.Replace(_filename, "Source");

            if (File.Exists(_filename + ".pyc"))
            {
                return new CompileResult
                {
                    HasErrors = false, 
                    Output = info,
                    PathToAssembly = string.Format("{0} {1}.pyc", AppSettings.Python3Path, _filename)
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
                if (File.Exists(_filename + ".py"))
                {
                    File.Delete(_filename + ".py");
                }
                if (File.Exists(_filename + ".pyc"))
                {
                    File.Delete(_filename + ".pyc");
                }
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
