using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using log4net;
using log4net.Util;
using SdojJudger.Compiler.Infrastructure;

namespace SdojJudger.Compiler
{
    public class CppCompiler : CompilerProvider
    {
        public CppCompiler(bool compileAsC)
        {
            _compileAsC = compileAsC;

            _fileExtension = compileAsC ? ".c" : ".cpp";

            _logger = LogManager.GetLogger(GetType());
        }

        public override CompileResult Compile(string source)
        {
            _logger.DebugExt(() => "Start compiling");


            var filename = GetTempFileNameWithoutExtension();
            _fullpath = Path.Combine(Path.GetTempPath(), filename);

            File.WriteAllText(_fullpath + _fileExtension, source, Encoding.Unicode);

            CompileSourceFile(_fullpath);

            var executableFile = _fullpath + ".exe";
            var log = File.ReadAllText(_fullpath + ".txt", Encoding.Default);
            log = log.Replace(_fullpath, "Source").Replace(filename, "Source");

            if (!File.Exists(executableFile))
            {
                return new CompileResult { HasErrors = true, Output = log, PathToAssembly = null };
            }

            return new CompileResult
            {
                HasErrors = false, 
                Output = log, 
                PathToAssembly = executableFile
            };
        }

        public override void Dispose()
        {
            if (File.Exists(_fullpath + _fileExtension))
            {
                File.Delete(_fullpath + _fileExtension);
            }
            if (File.Exists(_fullpath + ".obj"))
            {
                File.Delete(_fullpath + ".obj");
            }
            if (File.Exists(_fullpath + ".exe"))
            {
                File.Delete(_fullpath + ".exe");
            }
            if (File.Exists(_fullpath + ".txt"))
            {
                File.Delete(_fullpath + ".txt");
            }
        }

        private static string GetTempFileNameWithoutExtension()
        {
            return "judge-" + Guid.NewGuid();
        }

        private void CompileSourceFile(string sourceFile)
        {
            if (AppSettings.GccPath != null)
            {
                CompileByGcc(sourceFile);
                return;
            }
            if (AppSettings.VcCommandline != null)
            {
                CompileByVc(sourceFile);
                return;
            }
        }

        private void CompileByGcc(string sourceFile)
        {
            var pi = new ProcessStartInfo("cmd.exe")
            {
                UseShellExecute = false,
                CreateNoWindow = true, 
                RedirectStandardInput = true, 
                Arguments = "/Q /K " + "\"" + AppSettings.GccPath + "\""
            };

            string input;

            // C11
            // gcc -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s -x c -std=c11 -O2 -o %1.exe %1

            // C++11
            // g++ -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s -x c++ -std=c++1y -O2 -o %1.exe %1
            if (_compileAsC)
            {
                input =
                    string.Format(
                        "g++.exe -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s -x c -std=c11 -O2 -o {0}.exe {0}.c" +
                        " > {0}.txt 2>&1",
                        sourceFile) + Environment.NewLine +
                    "exit";
            }
            else
            {
                input =
                    string.Format(
                        "g++.exe -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s -x c++ -std=c++1y -O2 -o {0}.exe {0}.cpp" +
                        " > {0}.txt 2>&1",
                        sourceFile) + Environment.NewLine +
                    "exit";
            }


            var ps = Process.Start(pi);
            ps.StandardInput.WriteLine(input);
            _logger.DebugExt(() => input);
            ps.StandardInput.Close();

            ps.WaitForExit();
        }

        private void CompileByVc(string sourceFile)
        {
            string cl;

            if (_compileAsC)
            {
                cl = string.Format("cl \"{0}.c\" /MD /EHsc " +
                                   "/D \"ONLINE_JUDGE\" /D \"_CRT_SECURE_NO_DEPRECATE\" " +
                                   "/Fe:\"{0}.exe\" " +
                                   "/Fo:\"{0}.obj\" " +
                                   "/nologo /Ox " +
                                   "> \"{0}.txt\"", sourceFile) +
                     Environment.NewLine +
                     "exit";
            }
            else
            {
                cl = string.Format("cl \"{0}.cpp\" /MD /EHsc " +
                                   "/D \"ONLINE_JUDGE\" /D \"_CRT_SECURE_NO_DEPRECATE\" /D \"BOOST_ALL_DYN_LINK\" " +
                                   // "/D \"ONLINE_JUDGE\" /D \"_CRT_SECURE_NO_DEPRECATE\" " +
                                   "/Fe:\"{0}.exe\" " +
                                   "/Fo:\"{0}.obj\" " +
                                   "/nologo /Ox " +
                                   "> \"{0}.txt\"", sourceFile) +
                     Environment.NewLine +
                     "exit";
            }

            var arg = "/Q /K " + AppSettings.VcCommandline;
            var info = new ProcessStartInfo("cmd.exe")
            {
                Arguments = arg,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
            };
            var ps = Process.Start(info);

            ps.StandardInput.WriteLine(cl);
            _logger.DebugExt(() => cl);
            ps.WaitForExit();
        }

        private readonly bool _compileAsC;

        private readonly string _fileExtension;

        private readonly ILog _logger;

        private string _fullpath;
    }
}