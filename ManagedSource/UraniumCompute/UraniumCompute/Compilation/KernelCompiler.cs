﻿using System.Runtime.InteropServices;
using System.Text;
using UraniumCompute.Acceleration;
using UraniumCompute.Containers;
using UraniumCompute.Memory;

namespace UraniumCompute.Compilation;

/// <summary>
///     Compiler that is used for compiling compute shader source into backend's native code.
/// </summary>
public sealed class KernelCompiler : NativeObject
{
    public Desc Descriptor
    {
        get
        {
            IKernelCompiler_GetDesc(Handle, out var value);
            return value;
        }
    }

    internal KernelCompiler(IntPtr handle) : base(handle)
    {
    }

    public void Init(in Desc desc)
    {
        IKernelCompiler_Init(Handle, in desc).ThrowOnError("Couldn't initialize kernel compiler");
    }

    public unsafe NativeArray<byte> Compile(in Args args)
    {
        // We do not use 'out' here to avoid C++ code trying to deallocate uninitialized pointers.
        var bytecode = new NativeArrayBase();

        var unicodeSource = Encoding.UTF8.GetBytes(args.SourceCode);
        fixed (byte* source = unicodeSource)
        {
            var argsNative = new ArgsNative(new ArraySliceBase
            {
                pBegin = (sbyte*)source,
                pEnd = (sbyte*)(source + args.SourceCode.Length)
            }, args.OptimizationLevel, args.EntryPoint);
            IKernelCompiler_Compile(Handle, in argsNative, ref bytecode).ThrowOnError("Couldn't compile kernel code");
        }

        return new NativeArray<byte>(in bytecode);
    }

    [DllImport("UnCompute")]
    private static extern ResultCode IKernelCompiler_Init(IntPtr self, in Desc desc);

    [DllImport("UnCompute")]
    private static extern void IKernelCompiler_GetDesc(IntPtr self, out Desc desc);

    [DllImport("UnCompute")]
    private static extern ResultCode IKernelCompiler_Compile(IntPtr self, in ArgsNative args, ref NativeArrayBase bytecode);

    /// <summary>
    ///     Kernel compiler descriptor.
    /// </summary>
    /// <param name="Name">Compiler debug name.</param>
    /// <param name="SourceLang">Source code language.</param>
    /// <param name="TargetLang">Target code language.</param>
    [StructLayout(LayoutKind.Sequential)]
    public readonly record struct Desc(NativeString Name, KernelSourceLang SourceLang = KernelSourceLang.Hlsl,
        KernelTargetLang TargetLang = KernelTargetLang.SpirV);

    private readonly record struct ArgsNative(ArraySliceBase SourceCode, CompilerOptimizationLevel OptimizationLevel,
        NativeString EntryPoint);

    /// <summary>
    ///     Kernel compiler arguments that define a single compilation.
    /// </summary>
    /// <param name="SourceCode">Compute shader source code in a high-level language, e.g. HLSL.</param>
    /// <param name="OptimizationLevel">Compiler optimization level.</param>
    /// <param name="EntryPoint">Compute shader entry point.</param>
    public readonly record struct Args(string SourceCode, CompilerOptimizationLevel OptimizationLevel,
        NativeString EntryPoint);
}