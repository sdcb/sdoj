using System;
using System.Diagnostics;
using System.IO;
using SdojJudger.Compiler.Infrastructure;

namespace SdojJudger.Compiler
{
    public class JavaScriptCompiler : CompilerProvider
    {
        public override CompileResult Compile(string source)
        {
            _filename = GetTempFileNameWithoutExtension();
            File.WriteAllText(_filename + ".js", source);

            return new CompileResult
            {
                HasErrors = false,
                Output = null,
                PathToAssembly = string.Format("{0} {1}.js", AppSettings.NodeExePath, _filename)
            };
        }

        private string _filename;

        public override void Dispose()
        {
            if (_filename != null)
            {
                if (File.Exists(_filename + ".js"))
                {
                    File.Delete(_filename + ".js");
                }
            }
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
