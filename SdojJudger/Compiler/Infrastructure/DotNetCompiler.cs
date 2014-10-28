using System;
using System.CodeDom.Compiler;
using System.Diagnostics;

namespace SdojJudger.Compiler.Infrastructure
{
    public abstract class DotNetCompiler : CompilerProvider
    {
        static DotNetCompiler()
        {
            var windir = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            if (IntPtr.Size == 4)
            {
                NGenPath = windir + @"\Microsoft.NET\Framework\v4.0.30319\ngen.exe";
            }
            else if (IntPtr.Size == 8)
            {
                NGenPath = windir + @"\Microsoft.NET\Framework64\v4.0.30319\ngen.exe";
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        protected static void NGen(string pathToAssembly)
        {
            var si = new ProcessStartInfo(NGenPath, "install " + pathToAssembly)
            {
                CreateNoWindow = true, 
            };
            var ps = Process.Start(si);
            if (ps != null)
            {
                ps.WaitForExit();
            }
        }

        protected static void UnNGen(string pathToAssembly)
        {
            var si = new ProcessStartInfo(NGenPath, "uninstall " + pathToAssembly)
            {
                CreateNoWindow = true,
            };
            var ps = Process.Start(si);
        }

        protected static CompilerParameters GetCompilerOptions()
        {
            var options = new CompilerParameters {GenerateExecutable = true, CompilerOptions = "/optimize"};

            options.ReferencedAssemblies.Add("System.dll");
            options.ReferencedAssemblies.Add("System.Core.dll");
            options.ReferencedAssemblies.Add("System.Numerics.dll");

            return options;
        }

        private static readonly string NGenPath ;
    }
}
