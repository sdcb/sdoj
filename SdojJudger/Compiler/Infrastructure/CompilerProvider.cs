using System;
using SdojJudger.Models;
namespace SdojJudger.Compiler.Infrastructure
{
    public abstract class CompilerProvider : IDisposable
    {
        public static CompilerProvider GetCompiler(Languages language)
        {
            if (language == Languages.CSharp)
            {
                return new CSharpCompiler();
            }
            if (language == Languages.Cpp)
            {
                return new CppCompiler(compileAsC: false);
            }
            if (language == Languages.Vb)
            {
                return new VisualBasicCompiler();
            }
            if (language == Languages.C)
            {
                return new CppCompiler(compileAsC: true);
            }
            if (language == Languages.Python3)
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

        public abstract void Dispose();
    }
}