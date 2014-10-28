using System.CodeDom.Compiler;
using System.IO;
using Microsoft.CSharp;
using SdojJudger.Compiler.Infrastructure;

namespace SdojJudger.Compiler
{
    public class CSharpCompiler : CompilerProvider
    {
        public override CompileResult Compile(string source)
        {
            var csc = new CSharpCodeProvider();
            var options = new CompilerParameters {GenerateExecutable = true};
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            options.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            _asm = csc.CompileAssemblyFromSource(options, source);
            return new CompileResult(_asm);
        }

        private CompilerResults _asm;

        public override void Dispose()
        {
            if (File.Exists(_asm.PathToAssembly))
            {
                File.Delete(_asm.PathToAssembly);
            }
        }
    }
}
