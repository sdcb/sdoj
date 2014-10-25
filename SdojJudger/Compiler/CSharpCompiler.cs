using System.CodeDom.Compiler;
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
            var asm = csc.CompileAssemblyFromSource(options, source);
            return new CompileResult(asm);
        }
    }
}
