using System;
using System.IO;
using System.Text;
using System.Threading;
using log4net;
using SdojJudger.Models;
namespace SdojJudger.Compiler.Infrastructure
{
    public abstract class CompilerProvider : IDisposable
    {
        public static CompilerProvider GetCompiler(Languages language)
        {
            return language switch
            {
                Languages.CSharp     => (CompilerProvider)new CSharpCompiler(),
                Languages.Cpp        => new CppCompiler(compileAsC: false),
                Languages.C          => new CppCompiler(compileAsC: true),
                Languages.Vb         => new VisualBasicCompiler(), 
                Languages.Python3    => new Python3Compiler(), 
                Languages.Java       => new JavaCompiler(), 
                Languages.JavaScript => new JavaScriptCompiler(), 
                _                    => throw new NotImplementedException($"{nameof(language)}: {language}")
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
            else if (model.Language == Languages.JavaScript)
            {
                return AppSettings.NodeExePath != null;
            }
            // else
            return false;
        }

        public virtual Encoding GetEncoding() => Encoding.Default;

        public abstract CompileResult Compile(string source);

        public abstract void Dispose();

        public static void RetryDelete(string filename)
        {
            for (var retry = 0; File.Exists(filename); ++retry)
            {
                try
                {
                    File.Delete(filename);
                }
                catch (UnauthorizedAccessException)
                {
                    ILog logger = LogManager.GetLogger(typeof(CompilerProvider));
                    logger.Warn($"Delete {filename} failed, retry {retry + 1}...");
                    Thread.Sleep(100);
                }
            }
        }
    }
}