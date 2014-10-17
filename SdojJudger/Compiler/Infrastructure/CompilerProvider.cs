using SdojJudger.Models;
namespace SdojJudger.Compiler.Infrastructure
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
                return new CppCompiler(compileAsC: false);
            }
            if (model.Language == Languages.Vb)
            {
                return new VisualBasicCompiler();
            }
            if (model.Language == Languages.C)
            {
                return new CppCompiler(compileAsC: true);
            }
            if (model.Language == Languages.Python3)
            {
                return new Python3Compiler();
            }

            return null;
        }

        public static bool IsLanguageAvailable(SolutionPushModel model)
        {
            if (model.Language == Languages.C || model.Language == Languages.Cpp)
            {
                return AppSettings.VcCommandline != null ||
                       AppSettings.GccPath != null;
            }
            if (model.Language == Languages.Python3)
            {
                return AppSettings.Python3Path != null;
            }
            if (model.Language == Languages.CSharp || model.Language == Languages.Vb)
            {
                return true;
            }
            // else
            return false;
        }

        public abstract CompileResult Compile(string source);
    }
}