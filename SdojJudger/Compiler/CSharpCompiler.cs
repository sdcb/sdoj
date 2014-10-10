using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace SdojJudger.Compiler
{
    public class CSharpCompiler : CompilerProvider
    {
        public override CompilerResults Compile(string source)
        {
            var csc = new CSharpCodeProvider();
            var options = new CompilerParameters { GenerateExecutable = true };
            var asm = csc.CompileAssemblyFromSource(options, source);
            return asm;
        }
    }
}
