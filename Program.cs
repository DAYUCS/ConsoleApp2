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
                            public class Script1
                            {
                                public static int Sum(int a, int b)
                                {
                                    dynamic d = a + 1;
                                    return d + b;
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
                                        MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location)});

                var emitResult = compilation.Emit("EximScript1.dll", "EximScript1.pdb");
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
                Type calculator = compiledAssembly.GetType("Script1");
                object instance = Activator.CreateInstance(calculator);
                int result = (int)calculator.
                    InvokeMember("Sum", BindingFlags.InvokeMethod, null, instance, new object[] { 1, 1 });
                Console.WriteLine("Step:{0} -- Result: {0}", i, result); 
            }
            Console.ReadKey();
        }
    }
}
