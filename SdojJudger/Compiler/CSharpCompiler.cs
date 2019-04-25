using System.CodeDom.Compiler;
using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using SdojJudger.Compiler.Infrastructure;

namespace SdojJudger.Compiler
{
    public class CSharpCompiler : DotNetCompiler
    {
        public override CompileResult Compile(string source)
        {
            var csc = new CSharpCodeProvider();
            var options = GetCompilerOptions();
            options.ReferencedAssemblies.Add("Microsoft.CSharp.dll");

            _asm = csc.CompileAssemblyFromSource(options, source);

            return new CompileResult(_asm);
        }
        

        private CompilerResults _asm;

        public override void Dispose()
        {
            if (_asm != null)
            {
                RetryDelete(_asm.PathToAssembly);
            }
        }
    }
}
