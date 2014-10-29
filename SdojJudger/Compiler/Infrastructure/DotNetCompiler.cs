using System.CodeDom.Compiler;

namespace SdojJudger.Compiler.Infrastructure
{
    public abstract class DotNetCompiler : CompilerProvider
    {

        protected static CompilerParameters GetCompilerOptions()
        {
            var options = new CompilerParameters {GenerateExecutable = true, CompilerOptions = "/optimize"};

            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            options.ReferencedAssemblies.Add("System.Numerics.dll");

            return options;
        }
    }
}
