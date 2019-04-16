using System;
using SdojJudger.Models;
namespace SdojJudger.Compiler.Infrastructure
{
    public abstract class CompilerProvider : IDisposable
    {
        public static CompilerProvider GetCompiler(Languages language)
        {
            return language switch
            {
                Languages.CSharp  => (CompilerProvider)new CSharpCompiler(),
                Languages.Cpp     => new CppCompiler(compileAsC: false),
                Languages.C       => new CppCompiler(compileAsC: true),
                Languages.Vb      => new VisualBasicCompiler(), 
                Languages.Python3 => new Python3Compiler(), 
                Languages.Java    => new JavaCompiler(), 
                _                 => throw new NotImplementedException($"{nameof(language)}: {language}")
            };
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
            if (model.Language == Languages.Java)
            {
                return AppSettings.JdkBinPath != null;
            }
            // else
            return false;
        }

        public abstract CompileResult Compile(string source);

        public abstract void Dispose();
    }
}