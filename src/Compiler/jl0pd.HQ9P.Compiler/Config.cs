namespace jl0pd.HQ9P.Compiler;

using System.IO;

#nullable disable

public sealed class Config
{
    public FileInfo[] Reference { get; set; }
    public FileInfo[] Files { get; set; }
    public FileInfo Output { get; set; }
    public bool AttachDebugger { get; set; }
}
