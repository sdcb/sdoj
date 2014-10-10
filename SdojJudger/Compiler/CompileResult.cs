using System;
using System.CodeDom.Compiler;

namespace SdojJudger.Compiler
{
    public class CompileResult
    {
        public CompileResult()
        {
            
        }

        public CompileResult(CompilerResults compilerResults)
        {
            HasErrors = compilerResults.Errors.HasErrors;
            Output = string.Join(Environment.NewLine, compilerResults.Output);
            PathToAssembly = compilerResults.PathToAssembly;
        }

        public bool HasErrors { get; set; }

        public string Output { get; set; }

        public string PathToAssembly { get; set; }
    }
}
