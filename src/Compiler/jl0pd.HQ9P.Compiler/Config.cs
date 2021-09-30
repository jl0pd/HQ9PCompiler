namespace jl0pd.HQ9P.Compiler;

using System.IO;

#nullable disable

public sealed class Config
{
    public FileInfo[] Reference { get; set; }
    public FileInfo Input { get; set; }
    public FileInfo Output { get; set; }
    public bool AttachDebugger { get; set; }
    public Version Version { get; set; }
    public string Namespace { get; set; }
    public Mono.Cecil.ModuleKind OutputType { get; set; }
}
