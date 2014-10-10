using System.CodeDom.Compiler;
using Microsoft.VisualBasic;

namespace SdojJudger.Compiler
{
    public class VisualBasicCompiler : CompilerProvider
    {
        public override CompilerResults Compile(string source)
        {
            var vbc = new VBCodeProvider();
            var options = new CompilerParameters { GenerateExecutable = true };
            var asm = vbc.CompileAssemblyFromSource(options, source);
            return asm;
        }
    }
}