using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace SdojJudger.Compiler
{
    public class CSharpCompiler : CompilerProvider
    {
        public override CompileResult Compile(string sourceCode)
        {
            var csc = new CSharpCodeProvider();
            var options = new CompilerParameters { GenerateExecutable = true };
            var asm = csc.CompileAssemblyFromSource(options, sourceCode);
            
            return ToCompileResult(asm);
        }
    }
}
