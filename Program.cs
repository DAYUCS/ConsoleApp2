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
            if (Type.GetType("Eximbills.Script1, EximScript1, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") == null) {
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

                //Generate dll and pdb
                var emitResult = compilation.Emit("EximScript1.dll", "EximScript1.pdb");
                Console.WriteLine("Compiled.");

                //Generate Assembly and get type's Assembly Qualified Name
                using (var stream = new MemoryStream())
                {
                    var compileResult = compilation.Emit(stream);
                    Assembly compiledAssembly = Assembly.Load(stream.GetBuffer());
                    Type calculator = compiledAssembly.GetType("Eximbills.Script1");
                    Console.WriteLine($"Assembly Qualified Name: {calculator.AssemblyQualifiedName}");
                }
            }

            for (int i = 0; i < 1000000; i++) {
                Type calculator = Type.GetType("Eximbills.Script1, EximScript1, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (calculator == null)
                {
                    Console.WriteLine("Load assembly from dll...");
                    Assembly compiledAssembly = Assembly.LoadFrom("EximScript1.dll");
                    calculator = compiledAssembly.GetType("Eximbills.Script1");
                }
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
