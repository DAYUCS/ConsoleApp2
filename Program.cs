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

        private static readonly string codeNew = @"using System;
                            using ConsoleApp2;
                            namespace Eximbills
                            {
                                public class Script1
                                {
                                    public static void Any(EximMarco eximMarco)
                                    {
                                        eximMarco.Decrease();
                                    }
                                }
                            }";

        static void Main(string[] args)
        {
            Assembly compiledAssembly;

            if (Type.GetType("Eximbills.Script1, EximScript1, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null") == null)
            {
                CompileProgram(code, true);
            }

            // Type.GetType will load assembly from dll e.g. Assembly.LoadFrom(dll file)
            for (int i = 0; i < 10; i++) {
                Type calculator = Type.GetType("Eximbills.Script1, EximScript1, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                object instance = Activator.CreateInstance(calculator);
                EximMarco eximMarco = new EximMarco(i);
                calculator.InvokeMember("Any", BindingFlags.InvokeMethod, null, instance, new object[] { eximMarco });
                Console.WriteLine("Step:{0} -- Result: {1}", i, eximMarco.Count); 
            }

            // Load assembly with Assembly.Load(byte[]), this will cause memory leak if loading again and again!
            compiledAssembly = CompileProgram(codeNew, false);
            for (int i = 0; i < 10; i++)
            {
                Type calculator = compiledAssembly.GetType("Eximbills.Script1");
                object instance = Activator.CreateInstance(calculator);
                EximMarco eximMarco = new EximMarco(i);
                calculator.InvokeMember("Any", BindingFlags.InvokeMethod, null, instance, new object[] { eximMarco });
                Console.WriteLine("Step:{0} -- Result: {1}", i, eximMarco.Count);
            }

            //Get all assemblies loaded, there will be two EximScript1!
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Console.ReadKey();
        }

        private static Assembly CompileProgram(string code, bool usingDll)
        {
            Console.WriteLine("Compile Script...");
            Assembly compiledAssembly;
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
            if (usingDll)
            {
                compilation.Emit("dlls\\EximScript1.dll");
            }
            
            Console.WriteLine("Compiled.");

            if (usingDll) {
                compiledAssembly = Assembly.LoadFrom("dlls\\EximScript1.dll");
                /* Test Assembly.LoadFrom()
                for (int i = 0; i < 10; i++)
                {
                    Assembly.LoadFrom("dlls\\EximScript1.dll");
                }
                */
            } else {
                using (var stream = new MemoryStream())
                {
                    compilation.Emit(stream);
                    /* Test Assembly.Load()
                    for (int i =0; i < 10; i++)
                    {
                        Assembly.Load(stream.GetBuffer());
                    }
                    */
                    compiledAssembly = Assembly.Load(stream.GetBuffer());
                }
            }

            return compiledAssembly;
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

        public void Decrease()
        {
            Count--;
        }
    }
}
