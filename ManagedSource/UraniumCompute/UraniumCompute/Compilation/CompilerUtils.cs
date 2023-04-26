using UraniumCompute.Backend;
using UraniumCompute.Compiler.Decompiling;

namespace UraniumCompute.Compilation;

public static class CompilerUtils
{
    public static void CompileKernel(Delegate kernelMethod, KernelCompiler compiler, Kernel kernel,
        ResourceBinding resourceBinding, int batchSize = 1)
    {
        var sourceCode = MethodCompilation.Compile(kernelMethod, batchSize);
        using var bytecode = compiler.Compile(new KernelCompiler.Args(sourceCode, CompilerOptimizationLevel.Max, "main"));

        var parameters = kernelMethod.Method.GetParameters();
        var resources = new KernelResourceDesc[parameters.Length];
        for (var i = 0; i < parameters.Length; ++i)
        {
            var kind = GetResourceKind(parameters[i].ParameterType);
            resources[i] = new KernelResourceDesc(i, kind);
        }

        resourceBinding.Init(new ResourceBinding.Desc("Resource binding", resources));
        kernel.Init(new Kernel.Desc("Compute kernel", resourceBinding, bytecode[..]));
    }

    private static KernelResourceKind GetResourceKind(Type parameterType)
    {
        if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(Span<>))
        {
            return KernelResourceKind.RWBuffer;
        }

        return KernelResourceKind.ConstantBuffer;
    }
}
