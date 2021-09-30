namespace jl0pd.HQ9P.Compiler;

using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Mono.Cecil;

static class Program
{
    static Task<int> Main(string[] args)
    {
        if (args.Contains("--attach-debugger", StringComparer.OrdinalIgnoreCase))
        {
            System.Diagnostics.Debugger.Launch();
        }

        var root = new RootCommand
        {
            new Argument<FileInfo>("input"),
            new Option<bool>("--attach-debugger"),
            new Option<FileInfo>(new [] { "-o", "--output" }),
            new Option<Version>(new [] { "-v", "--version" }),
            new Option<string>(new [] { "-n", "--namespace" }),
            new Option<FileInfo[]>(new [] { "-r", "--reference" }),
            new Option<ModuleKind>(new [] { "-t", "--output-type" },
                                   c => c.Tokens.Single().Value.Match(StringComparer.OrdinalIgnoreCase,
                                                                      ("Library", ModuleKind.Dll),
                                                                      ("Exe", ModuleKind.Console),
                                                                      ("WinExe", ModuleKind.Windows),
                                                                      ("Module", ModuleKind.NetModule))),
        };
        root.Handler = CommandHandler.Create(Compiler.StartCompilation);

        return root.InvokeAsync(args);
    }

    // C# doesn't have native support for case insensitive switch
    static T Match<T>(this string input, IEqualityComparer<string> comparer, params (string, T)[] matchers)
    {
        foreach (var (key, value) in matchers)
        {
            if (comparer.Equals(key, input))
            {
                return value;
            }
        }

        throw new SwitchExpressionException();
    }
}
