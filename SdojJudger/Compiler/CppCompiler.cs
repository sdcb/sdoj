﻿using System;
using System.Diagnostics;
using System.IO;
using log4net;
using log4net.Util;

namespace SdojJudger.Compiler
{
    public class CppCompiler : CompilerProvider
    {
        public CppCompiler(bool compileAsC)
        {
            CompileAsC = compileAsC;

            _fileExtension = compileAsC ? ".c" : ".cpp";

            _logger = LogManager.GetLogger(GetType());
        }

        public override CompileResult Compile(string source)
        {
            _logger.DebugExt(() => "Start compiling");

            var filename = GetTempFileNameWithoutExtension();

            File.WriteAllText(filename + _fileExtension, source);

            CompileSourceFile(filename);

            var executableFile = filename + ".exe";
            var log = File.ReadAllText(filename + ".txt");
            log = log.Replace(filename, "source");

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

        private static string GetTempFileNameWithoutExtension()
        {
            var filename = Path.Combine(
                Path.GetTempPath(),
                "judge-" + Guid.NewGuid());
            return filename;
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
                Arguments = "/Q"
            };

            string input;

            // C11
            // gcc -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s -x c -std=c11 -O2 -o %1.exe %1

            // C++11
            // g++ -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s -x c++ -std=c++1y -O2 -o %1.exe %1
            if (CompileAsC)
            {
                var gcc = Path.Combine(AppSettings.GccPath, "gcc.exe");
                input =
                    "setlocal" + Environment.NewLine +
                    "set path=%path%;" + AppSettings.GccPath + Environment.NewLine +
                    string.Format(
                        "{0} -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s -x c -std=c11 -O2 -o {1}.exe {1}.c" +
                        " > {1}.txt 2>&1",
                        gcc,
                        sourceFile) + Environment.NewLine +
                    "exit";
            }
            else
            {
                var gcc = Path.Combine(AppSettings.GccPath, "g++.exe");
                input =
                    "setlocal" + Environment.NewLine +
                    "set path=%path%;" + AppSettings.GccPath + Environment.NewLine +
                    string.Format(
                        "{0} -static -fno-strict-aliasing -DONLINE_JUDGE -lm -s -x c++ -std=c++1y -O2 -o {1}.exe {1}.cpp" +
                        " > {1}.txt 2>&1",
                        gcc,
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
            var arg = "/Q /K " + "\"" + AppSettings.VcCommandline + "\"";
            string cl;

            if (CompileAsC)
            {
                cl = string.Format("cl \"{0}.c\" /Fe:\"{0}.exe\" /Fo:\"{0}.obj\" /nologo /Ox > \"{0}.txt\"", sourceFile) +
                     Environment.NewLine +
                     "exit";
            }
            else
            {
                cl = string.Format("cl \"{0}.cpp\" /Fe:\"{0}.exe\" /Fo:\"{0}.obj\" /nologo /Ox > \"{0}.txt\"", sourceFile) +
                     Environment.NewLine +
                     "exit";
            }
            
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

        public bool CompileAsC { get; private set; }

        private readonly string _fileExtension;

        private readonly ILog _logger;
    }
}