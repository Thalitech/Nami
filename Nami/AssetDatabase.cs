using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Nami
{
    internal class AssetDatabase
    {
        private static FileSystemWatcher system;
        public static Assembly compiledAssembly { get; private set; } = null;

        internal static void Init()
        {
            system = new FileSystemWatcher();
            system.Path = Directory.CreateDirectory("Resources/UserCommands").FullName;
            system.NotifyFilter = NotifyFilters.LastAccess
                               | NotifyFilters.LastWrite
                               | NotifyFilters.FileName
                               | NotifyFilters.DirectoryName;
            system.Filter = "*.cs";
            system.Changed += (a, b) => Recompile();
            system.Created += (a, b) => Recompile();
            system.Deleted += (a, b) => Recompile();
            system.EnableRaisingEvents = true;
            Recompile();            
        }

        private static void Recompile()
        {
            DisplayCSharpCompilerInfo();
            compiledAssembly = null;
            var files = Directory.GetFiles(system.Path, "*.cs", SearchOption.AllDirectories);
            if (files.Length <= 0) return;

            CSharpCodeProvider codeProvider = new CSharpCodeProvider(new Dictionary<string, string>()
            {
                { "CompilerVersion", "v9.0" }
            });
            CompilerParameters cpParams = new CompilerParameters();
            cpParams.GenerateExecutable = false;
            cpParams.GenerateInMemory = true;
            cpParams.IncludeDebugInformation = true;
            cpParams.TreatWarningsAsErrors = false;
            cpParams.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            var dir = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.FullName;
            cpParams.ReferencedAssemblies.Add("System.dll");
            cpParams.ReferencedAssemblies.Add("netstandard.dll");
            cpParams.ReferencedAssemblies.AddRange(typeof(Program).Assembly.GetReferencedAssemblies().Select(x => $"{ x.Name}.dll").ToArray());
            try
            {
                CompilerResults results = codeProvider.CompileAssemblyFromFile(cpParams, files);
                if (results.Errors.HasErrors)
                {
                    Console.WriteLine("|=== USER CODE ERROR ===|");
                    foreach (var error in results.Errors)
                    {
                        var errorText = error.ToString().Replace(" : ", "\n");
                        Console.WriteLine(errorText);
                    }
                    Program.instance.commandCode = Program.CommandCode.stop;
                    return;
                }
                compiledAssembly = results.CompiledAssembly;
                Program.instance.commandCode = Program.CommandCode.Reset;
                Console.WriteLine("|=== USER CODE COMPILED SUCCESSFULLY ===>");
                Thread.Sleep(3000);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("|=== USER CODE ERROR ===|");
                Console.WriteLine($"Unable to complile code\n{ex}");
                compiledAssembly = null;
            }
        }

        static void DisplayCSharpCompilerInfo()
        {
            Dictionary<string, string> provOptions =
            new Dictionary<string, string>();

            provOptions.Add("CompilerVersion", "v9.0");
            // Get the provider for Microsoft.CSharp
            CSharpCodeProvider csProvider = new CSharpCodeProvider(provOptions);

            // Display the C# language provider information.
            Console.WriteLine("CSharp provider: {0}",
                csProvider.ToString());
            Console.WriteLine("Provider hash code: {0}",
                csProvider.GetHashCode().ToString());
            Console.WriteLine("Default file extension: {0}",
                csProvider.FileExtension);

            Console.WriteLine();
        }
    }
}