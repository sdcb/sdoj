using System.CodeDom.Compiler;
using System.IO;
using Microsoft.VisualBasic;
using SdojJudger.Compiler.Infrastructure;

namespace SdojJudger.Compiler
{
    public class VisualBasicCompiler : DotNetCompiler
    {
        public override CompileResult Compile(string source)
        {
            var vbc = new VBCodeProvider();
            var options = GetCompilerOptions();
            options.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll");
            _asm = vbc.CompileAssemblyFromSource(options, source);

            if (!_asm.Errors.HasErrors)
            {
                NGen(_asm.PathToAssembly);
            }

            return new CompileResult(_asm);
        }

        public override void Dispose()
        {
            if (File.Exists(_asm.PathToAssembly))
            {
                File.Delete(_asm.PathToAssembly);
                UnNGen(_asm.PathToAssembly);
            }
        }

        private CompilerResults _asm;
    }
}