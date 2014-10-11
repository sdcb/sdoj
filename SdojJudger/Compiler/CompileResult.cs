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
            foreach (var output in compilerResults.Output)
            {
                Output += output;
            }
            PathToAssembly = compilerResults.PathToAssembly;
        }

        public bool HasErrors { get; set; }

        public string Output { get; set; }

        public string PathToAssembly { get; set; }
    }
}
