using System.CodeDom.Compiler;
using Microsoft.VisualBasic;
using SdojJudger.Compiler.Infrastructure;

namespace SdojJudger.Compiler
{
    public class VisualBasicCompiler : CompilerProvider
    {
        public override CompileResult Compile(string source)
        {
            var vbc = new VBCodeProvider();
            var options = new CompilerParameters { GenerateExecutable = true };
            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            options.ReferencedAssemblies.Add("Microsoft.VisualBasic.dll");
            var asm = vbc.CompileAssemblyFromSource(options, source);
            return new CompileResult(asm);
        }
    }
}