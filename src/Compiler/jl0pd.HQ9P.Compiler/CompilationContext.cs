namespace jl0pd.HQ9P.Compiler;

using System;
using System.IO;
using System.Linq;
using System.Text;
using Mono.Cecil;

internal class CompilationContext
{
    private CompilationContext(TokenType token, AssemblyDefinition[] referenceAssemblies, TypeDefinition[] types, TypeDefinition mainType)
    {
        Token = token;
        ReferenceAssemblies = referenceAssemblies;
        Types = types;
        MainType = mainType;

        MainType.BaseType = ImportType("System.Object");
    }

    public TokenType Token { get; }
    public AssemblyDefinition[] ReferenceAssemblies { get; }
    public TypeDefinition[] Types { get; }
    public TypeDefinition MainType { get; }

    public TypeReference ImportType(string namespaceQualifiedName)
        => MainType.Module.ImportReference(GetExportedType(namespaceQualifiedName));

    public MethodReference Import(MethodDefinition method)
        => MainType.Module.ImportReference(method);

    private TypeDefinition GetExportedType(string namespaceQualifiedName)
        => Types.First(t => string.Equals(t.FullName, namespaceQualifiedName, StringComparison.OrdinalIgnoreCase));

    public static CompilationContext Create(FileInfo[] referenceAssemblies, FileInfo codeFile)
    {
        string code = File.ReadAllText(codeFile.FullName, Encoding.UTF8);
        var token = code switch
        {
            "H" or "h" => TokenType.H,
            "Q" or "q" => TokenType.Q,
            "9" or "N" or "n" => TokenType.N,
            "+" or "P" or "p" => TokenType.P,
            _ => throw new ArgumentException("Invalid character", nameof(codeFile)),
        };

        var assemblies = referenceAssemblies.Select(r => AssemblyDefinition.ReadAssembly(r.FullName)).ToArray();
        var types = assemblies.SelectMany(a => a.Modules).SelectMany(m => m.GetTypes()).ToArray();

        var resAsm = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition("HQ9PAssembly", new Version(0, 0, 0, 0)), "<module>", ModuleKind.Dll);
        var type = new TypeDefinition("HQ9P", "Program", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
        resAsm.MainModule.Types.Add(type);

        return new CompilationContext(token, assemblies, types, type);
    }
}
