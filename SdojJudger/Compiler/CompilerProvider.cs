using System.CodeDom.Compiler;
using SdojJudger.Models;

namespace SdojJudger.Compiler
{
    public abstract class CompilerProvider
    {
        public static CompilerProvider GetCompiler(SolutionPushModel model)
        {
            if (model.Language == Languages.CSharp)
            {
                return new CSharpCompiler();
            }
            if (model.Language == Languages.Cpp)
            {
                return new VisualBasicCompiler();
            }
            if (model.Language == Languages.Vb)
            {
                return new VisualBasicCompiler();
            }

            return null;
        }

        public abstract CompileResult Compile(string source);
    }
}