using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Dynamic;
using System.IO;
using System.Reflection;

namespace ConsoleApp2
{
    class Program
    {
        private static readonly string code = @"using System;
                            using ConsoleApp2;
                            namespace Eximbills
                            {
                                public class Script1
                                {
                                    public static void Any(EximMarco eximMarco)
                                    {
                                        eximMarco.Increase();
                                    }
                                }
                            }";

        static void Main(string[] args)
        {
            if (!File.Exists("EximScript1.dll")) {
                Console.WriteLine("Compile Script into dll...");
                var tree = SyntaxFactory.ParseSyntaxTree(code);
                var compilation = CSharpCompilation.Create(
                    "EximScript1",
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                    syntaxTrees: new[] { tree },
                    references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                                        MetadataReference.CreateFromFile(typeof(ExpandoObject).Assembly.Location),
                                        MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location),
                                        MetadataReference.CreateFromFile(typeof(EximMarco).Assembly.Location)});

                var emitResult = compilation.Emit("EximScript1.dll", "EximScript1.pdb");
                Console.WriteLine("Compiled.");
            }
            Assembly compiledAssembly;
            /*
            using (var stream = new MemoryStream())
            {
                var compileResult = compilation.Emit(stream);
                compiledAssembly = Assembly.Load(stream.GetBuffer());
            }
            */

            for (int i = 0; i < 1000000; i++) { 
                compiledAssembly = Assembly.LoadFrom("EximScript1.dll");
                Type calculator = compiledAssembly.GetType("Eximbills.Script1");
                object instance = Activator.CreateInstance(calculator);
                EximMarco eximMarco = new EximMarco(i);
                calculator.InvokeMember("Any", BindingFlags.InvokeMethod, null, instance, new object[] { eximMarco });
                Console.WriteLine("Step:{0} -- Result: {1}", i, eximMarco.Count); 
            }
            Console.ReadKey();
        }
    }

    public class EximMarco
    {
        public EximMarco()
        {
            Count = 0;
        }

        public EximMarco(int i)
        {
            Count = i;
        }
        public int Count { get; set; }

        public void Increase()
        {
            Count++;
        }
    }
}
