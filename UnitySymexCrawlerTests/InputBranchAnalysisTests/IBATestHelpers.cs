using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler.TypeSystem;
using ICSharpCode.Decompiler.IL;

namespace UnitySymexCrawler.Tests
{
    public class IBATestHelpers
    {

        public static InputBranchAnalysis CreateInputBranchAnalysis(string entryPointClassFullName, string entryPointMethodName)
        {
            string assemblyPath = @"..\..\..\..\TestCases\bin\Debug\netcoreapp3.1\TestCases.dll";
            var peFile = new PEFile(assemblyPath,
                new FileStream(assemblyPath, FileMode.Open, FileAccess.Read),
                streamOptions: PEStreamOptions.PrefetchEntireImage);
            var assemblyResolver = new UniversalAssemblyResolver(assemblyPath, true,
                peFile.DetectTargetFrameworkId(),
                peFile.DetectRuntimePack(),
                PEStreamOptions.PrefetchMetadata,
                MetadataReaderOptions.None);
            var settings = new DecompilerSettings();
            var decompiler = new CSharpDecompiler(peFile, assemblyResolver, settings);
            IType program = decompiler.TypeSystem.MainModule.Compilation.FindType(new FullTypeName(entryPointClassFullName));
            IMethod method = program.GetMethods().Where(m => m.Name == entryPointMethodName).First();
            return new InputBranchAnalysis(method, new MethodPool());
        }

        public static ILFunction FetchMethod(IMethod m, MethodPool pool)
        {
            return (ILFunction)pool.MethodEntryPoint(m).block.Parent.Parent;
        }

        public static ILInstruction Entrypoint(ILFunction func)
        {
            BlockContainer bc = (BlockContainer)func.Body;
            return bc.Blocks[0].Instructions[0];
        }

        public static ILInstruction FindInstruction(ILFunction func, string rep)
        {
            BlockContainer bc = (BlockContainer)func.Body;
            foreach (Block block in bc.Blocks)
            {
                foreach (ILInstruction inst in block.Instructions)
                {
                    if (inst.ToString().StartsWith(rep))
                    {
                        return inst;
                    }
                }
            }
            throw new System.Exception("could not find instruction '" + rep + "'");
        }
    }
}
