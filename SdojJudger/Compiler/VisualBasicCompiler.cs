using System.CodeDom.Compiler;
using Microsoft.VisualBasic;

namespace SdojJudger.Compiler
{
    public class VisualBasicCompiler : CompilerProvider
    {
        public override CompileResult Compile(string sourceCode)
        {
            var vbc = new VBCodeProvider();
            var options = new CompilerParameters { GenerateExecutable = true };
            var asm = vbc.CompileAssemblyFromSource(options, sourceCode);
            return ToCompileResult(asm);
        }
    }
}