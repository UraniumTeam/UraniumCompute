using UraniumCompute.Backend;
using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compilation;

public static class CompilerUtils
{
    public static void CompileKernel(Delegate kernelMethod, KernelCompiler compiler, Kernel kernel,
        ResourceBinding resourceBinding)
    {
        var compilation = MethodCompilation.Create(kernelMethod);
        var result = compilation.Compile();
        var sourceCode = result.HlslCode!;

        using var bytecode = compiler.Compile(new KernelCompiler.Args(sourceCode, CompilerOptimizationLevel.Max, "main"));

        var parameters = kernelMethod.Method.GetParameters();
        var resources = new KernelResourceDesc[parameters.Length];
        for (var i = 0; i < parameters.Length; ++i)
        {
            resources[i] = new KernelResourceDesc(i, KernelResourceKind.RWBuffer);
        }

        resourceBinding.Init(new ResourceBinding.Desc("Resource binding", resources));
        kernel.Init(new Kernel.Desc("Compute kernel", resourceBinding, bytecode[..]));
    }
}
