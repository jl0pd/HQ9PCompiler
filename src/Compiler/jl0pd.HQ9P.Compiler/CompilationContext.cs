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

    public static CompilationContext Create(Config cfg)
    {
        string code = File.ReadAllText(cfg.Input.FullName, Encoding.UTF8);
        var token = code switch
        {
            "H" or "h" => TokenType.H,
            "Q" or "q" => TokenType.Q,
            "9" or "N" or "n" => TokenType.N,
            "+" or "P" or "p" => TokenType.P,
            _ => throw new ArgumentException("Invalid input character", nameof(cfg)),
        };

        var assemblies = cfg.Reference.AsParallel().Select(r => AssemblyDefinition.ReadAssembly(r.FullName)).ToArray();
        var types = assemblies.AsParallel().SelectMany(a => a.Modules).SelectMany(m => m.GetTypes()).ToArray();

        var @namespace = Path.GetFileNameWithoutExtension(cfg.Output.FullName);

        var resAsm = AssemblyDefinition.CreateAssembly(new AssemblyNameDefinition(@namespace, cfg.Version), "<module>", cfg.OutputType);
        var type = new TypeDefinition(cfg.Namespace ?? @namespace, "Program", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
        resAsm.MainModule.Types.Add(type);

        return new CompilationContext(token, assemblies, types, type);
    }
}
