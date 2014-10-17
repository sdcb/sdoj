﻿using SdojJudger.Models;
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
                return new CppCompiler(compileAsC:false);
            }
            if (model.Language == Languages.Vb)
            {
                return new VisualBasicCompiler();
            }
            if (model.Language == Languages.C)
            {
                return new CppCompiler(compileAsC: true);
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
            // else
            return true;
        }

        public abstract CompileResult Compile(string source);
    }
}