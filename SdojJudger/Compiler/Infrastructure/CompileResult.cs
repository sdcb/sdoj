using System.CodeDom.Compiler;

namespace SdojJudger.Compiler.Infrastructure
{
    public class CompileResult
    {
        public CompileResult()
        {
            
        }

        public CompileResult(CompilerResults compilerResults)
        {
            HasErrors = compilerResults.Errors.HasErrors;
            foreach (CompilerError output in compilerResults.Errors)
            {
                output.FileName = "Source";
                Output += output;
            }
            
            
            PathToAssembly = compilerResults.PathToAssembly;
        }

        public bool HasErrors { get; set; }

        public string Output { get; set; }

        public string PathToAssembly { get; set; }
    }
}
