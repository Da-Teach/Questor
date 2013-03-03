using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using CommandLine;
using CommandLine.Text;
using Injector;
using System.Diagnostics;

namespace TestApp
{
    partial class Program
    {
        private static void Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLine.Parser(with => with.HelpWriter = Console.Error);

            // ReadKey() is there for debugging.
            if (parser.ParseArgumentsStrict(args, options, () => { Console.ReadKey();  Environment.Exit(-2); }))
            {
                Console.WriteLine("inject: {0}", options.InputFile);
                Console.WriteLine("function: {0}", options.FuncName);
                Console.WriteLine("args: {0}", options.FuncArgs);
                Console.WriteLine("pid: {0}", options.ProcID);
                Console.WriteLine("name: {0}", options.ProcName);
                Console.WriteLine("launch: {0}", options.ProcLaunch);

                InjectorLib injector = new InjectorLib();
                bool injected = false;
                UInt32 retCode = 0;
                if( !string.IsNullOrWhiteSpace(options.ProcName) )
                {
                    Process[] procs = Process.GetProcessesByName(options.ProcName);
                    foreach (Process proc in procs)
                    {
                        Console.WriteLine("Injecting '{0}' into process {1}", options.InputFile, proc.Id);
                        injected = injector.InjectAndRun((UInt32)proc.Id, options.InputFile, options.FuncName, options.FuncArgs, ref retCode);
                    }
                }
                else if( options.ProcID != 0 )
                {
                    Console.WriteLine("Injecting '{0}' into process {1}", options.InputFile, options.ProcID);
                    injected = injector.InjectAndRun(options.ProcID, options.InputFile, options.FuncName, options.FuncArgs, ref retCode);
                }
                else if( !string.IsNullOrWhiteSpace(options.ProcLaunch) )
                {
                    Console.WriteLine("Launching '{0}' and injecting '{1}'", options.ProcLaunch, options.InputFile);
                    injected = injector.LaunchAndInject(options.ProcLaunch, options.InputFile, options.FuncName, options.FuncArgs, ref retCode);
                }
                Environment.Exit(injected ? 0 : -1);
            }
        }

        private sealed class Options
        {
            [Option('i', "inject", MetaValue="FILE", Required = true, HelpText = "Input file to inject into process.")]
            public string InputFile { get; set; }

            [Option('f', "function", MetaValue = "NAME", DefaultValue = "", HelpText = "A function to call from the injected module.")]
            public string FuncName { get; set; }

            [Option('a', "args", MetaValue = "ARGS", DefaultValue = "", HelpText = "Arguments to pass into called function.")]
            public string FuncArgs { get; set; }

            [Option('p', "pid", MetaValue = "PID", MutuallyExclusiveSet = "proc", HelpText = "The ID of the process to inject.")]
            public UInt32 ProcID { get; set; }

            [Option('n', "name", MetaValue = "NAME", MutuallyExclusiveSet = "proc", HelpText = "The name of the process to inject.")]
            public string ProcName { get; set; }

            [Option('l', "launch", MetaValue = "FILE", MutuallyExclusiveSet = "proc", HelpText = "The path to a process to launch.")]
            public string ProcLaunch { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

    }
}