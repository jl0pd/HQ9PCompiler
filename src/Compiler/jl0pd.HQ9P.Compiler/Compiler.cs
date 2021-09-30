namespace jl0pd.HQ9P.Compiler;

using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.OpCodes;

public static class Compiler
{
    public static void StartCompilation(Config cfg)
    {
        Compile(CompilationContext.Create(cfg), cfg);
    }

    static void Compile(CompilationContext ctx, Config cfg)
    {
        var @void = ctx.ImportType("System.Void");
        var mainMethod = new MethodDefinition("Main", MethodAttributes.Static | MethodAttributes.Public, @void);

        switch (ctx.Token)
        {
            case TokenType.H:
                CompileHelloWorld(ctx, mainMethod);
                break;
            case TokenType.Q:
                CompileQuine(ctx, mainMethod);
                break;
            case TokenType.N:
                CompileNine(ctx, mainMethod);
                break;
            case TokenType.P:
                CompilePlus(ctx, mainMethod);
                break;
            default:
                throw new InvalidEnumArgumentException();
        }

        ctx.MainType.Methods.Add(mainMethod);
        ctx.MainType.Module.Assembly.EntryPoint = mainMethod;
        ctx.MainType.Module.Assembly.Write(cfg.Output.FullName);
    }

    private static void CompilePlus(CompilationContext ctx, MethodDefinition mainMethod)
    {
        var intType = ctx.ImportType("System.Int32");
        var field = new FieldDefinition("_counter", FieldAttributes.Private | FieldAttributes.Static, intType);
        ctx.MainType.Fields.Add(field);

        var il = mainMethod.Body.GetILProcessor();

        il.Emit(Ldsfld, field);
        il.Emit(Ldc_I4_1);
        il.Emit(Add);
        il.Emit(Stsfld, field);

        il.Emit(Ret);
    }

    private static void CompileNine(CompilationContext ctx, MethodDefinition mainMethod)
    {
        var consoleMethods = ctx
                                .ImportType("System.Console")
                                .Resolve()
                                .Methods;

        var writeLine = ctx.Import(consoleMethods
                                    .Single(m => m.Name == "WriteLine"
                                              && m.Parameters.Count == 1
                                              && m.Parameters[0].ParameterType.FullName == "System.String"));

        var writeLineStrObj = ctx.Import(consoleMethods
                                            .Single(m => m.Name == "WriteLine"
                                                      && m.Parameters.Count == 2
                                                      && m.Parameters[0].ParameterType.FullName == "System.String"
                                                      && m.Parameters[1].ParameterType.FullName == "System.Object"));

        TypeReference int32 = ctx.ImportType("System.Int32");
        mainMethod.Body.Variables.Add(new VariableDefinition(int32));

        // this is stupid
        var il = mainMethod.Body.GetILProcessor();

        var x00 = Instruction.Create(Ldc_I4, 100);
        var x02 = Instruction.Create(Stloc_0);
        var x03 = default(Instruction);

        var x05 = Instruction.Create(Ldstr, "{0} bottles of beer on the wall, {0} bottles of beer"); // Console.WriteLine(str, i);
        var x0a = Instruction.Create(Ldloc_0);
        var x0b = Instruction.Create(Box, int32);
        var x10 = Instruction.Create(Call, writeLineStrObj);

        var x15 = Instruction.Create(Ldstr, "Take one down and pass it around, {0} bottles of beer on the wall"); // Console.WriteLine(str, i - 1);
        var x1a = Instruction.Create(Ldloc_0);
        var x1b = Instruction.Create(Ldc_I4_1);
        var x1c = Instruction.Create(Sub);
        var x1d = Instruction.Create(Box, int32);
        var x22 = Instruction.Create(Call, writeLineStrObj);

        var x27 = Instruction.Create(Ldloc_0); // i--
        var x28 = Instruction.Create(Ldc_I4_1);
        var x29 = Instruction.Create(Sub);
        var x2a = Instruction.Create(Stloc_0);

        var x2b = Instruction.Create(Ldloc_0); // i > 0
        var x2c = Instruction.Create(Ldc_I4_1);
        var x2d = default(Instruction);

        var x2f = Instruction.Create(Ldstr, "1 bottle of beer on the wall, 1 bottle of beer");
        var x34 = Instruction.Create(Call, writeLine);

        var x39 = Instruction.Create(Ldstr, "Take one down and pass it around, no bottles of beer on the wall");
        var x3e = Instruction.Create(Call, writeLine);

        var x43 = Instruction.Create(Ret);

        x03 = Instruction.Create(Br_S, x2b);
        x2d = Instruction.Create(Bgt_S, x05);

        il.Append(x00);
        il.Append(x02);

        il.Append(x03);
        il.Append(x05);
        il.Append(x0a);
        il.Append(x0b);
        il.Append(x10);

        il.Append(x15);
        il.Append(x1a);
        il.Append(x1b);
        il.Append(x1c);
        il.Append(x1d);
        il.Append(x22);

        il.Append(x27);
        il.Append(x28);
        il.Append(x29);
        il.Append(x2a);

        il.Append(x2b);
        il.Append(x2c);
        il.Append(x2d);

        il.Append(x2f);
        il.Append(x34);

        il.Append(x39);
        il.Append(x3e);
        il.Append(x43);
    }

    // Since it's not obvious what to use to create quine, I'm going just print 'Q'.
    // Should I write compiler code - only this method / this class / entire project (including .csproj)
    // OR msil OR generated assembly OR something else?
    // I understand that this is not honest implementation
    private static void CompileQuine(CompilationContext ctx, MethodDefinition mainMethod)
        => EmitWriteLine("Q", mainMethod, ctx);

    private static void CompileHelloWorld(CompilationContext ctx, MethodDefinition mainMethod)
        => EmitWriteLine("Hello world!", mainMethod, ctx);

    private static void EmitWriteLine(string str, MethodDefinition method, CompilationContext ctx)
    {
        var writeLineRef = ctx
                            .ImportType("System.Console")
                            .Resolve()
                            .Methods
                            .Single(m => m.Name == "WriteLine"
                                      && m.Parameters.Count == 1
                                      && m.Parameters[0].ParameterType.FullName == "System.String");

        var writeLine = ctx.Import(writeLineRef);

        var il = method.Body.GetILProcessor();
        il.Emit(Ldstr, str);
        il.Emit(Call, writeLine);
        il.Emit(Ret);
    }
}
