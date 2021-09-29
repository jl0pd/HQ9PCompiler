namespace jl0pd.HQ9P.Compiler;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

static class Program
{
    static Task<int> Main(string[] args)
    {
        var root = new RootCommand
        {
            new Argument<FileInfo[]>("files"),
            new Option<bool>("--attach-debugger"),
            new Option<string>(new [] { "-o", "--output" }),
            new Option<FileInfo[]>(new [] { "-r", "--reference" }),
        };
        root.Handler = CommandHandler.Create(Compiler.StartCompilation);

        return root.InvokeAsync(args);
    }
}
