using System.CodeDom.Compiler;

namespace SdojJudger.Compiler
{
    public class CppCompiler : CompilerProvider
    {
        public override CompileResult Compile(string source)
        {
            var result = new CompilerResults(null);
            result.Errors.Add(new CompilerError());
            return new CompileResult(result);
        }
    }
}