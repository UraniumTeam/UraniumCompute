#pragma once
#include <UnCompute/Containers/HeapArray.h>
#include <UnCompute/Memory/Object.h>

namespace UN
{
    //! \brief Source language of compute shader compilation.
    enum class KernelSourceLang
    {
        Hlsl //!< High-Level Shader Language, cs_6_0 profile
    };

    //! \brief Target language of compute shader compilation.
    enum class KernelTargetLang
    {
        SpirV //!< Standard Portable Intermediate Representation, used in Vulkan backend.
    };

    //! \brief Kernel compiler descriptor.
    struct KernelCompilerDesc
    {
        const char* Name            = nullptr;                 //!< Compiler debug name.
        KernelSourceLang SourceLang = KernelSourceLang::Hlsl;  //!< Source code language.
        KernelTargetLang TargetLang = KernelTargetLang::SpirV; //!< Target code language.
    };

    //! \brief Text encoding.
    enum class TextEncoding
    {
        Ascii, //!< ASCII encoding: number of characters in the text is equal to the number of bytes.
        Utf8   //!< UTF-8 encoding: ASCII-compatible, variable-width.
    };

    //! Flags used to control compiler optimization level.
    enum class CompilerOptimizationLevel
    {
        None,    //!< No optimization.
        O1,      //!< Level 1 optimization.
        O2,      //!< Level 2 optimization.
        O3,      //!< Level 3 optimization.
        Max = O3 //!< Maximum level of optimization supported by the compiler.
    };

    //! \brief Kernel compiler arguments that define a single compilation.
    struct KernelCompilerArgs
    {
        ArraySlice<UInt8> SourceCode; //!< Compute shader source code in a high-level language, e.g. HLSL.
        TextEncoding SourceEncoding                 = TextEncoding::Ascii;            //!< Source code text encoding.
        CompilerOptimizationLevel OptimizationLevel = CompilerOptimizationLevel::Max; //!< Compiler optimization level.
        const char* EntryPoint                      = "main";                         //!< Compute shader entry point.
    };

    //! \brief An interface for kernel compiler that is used for compiling compute shader source into backend's native code.
    class IKernelCompiler : public IObject
    {
    public:
        using DescriptorType = KernelCompilerDesc;

        [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;

        //! \brief Creates and initializes a kernel compiler.
        //!
        //! \param desc - Kernel compiler descriptor.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode Init(const DescriptorType& desc) = 0;

        //! \brief Compile a compute kernel into target language.
        //!
        //! \param args    - Compiler arguments.
        //! \param pResult - A pointer to an array where the compiled bytecode will be written.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode Compile(const KernelCompilerArgs& args, HeapArray<Int8>* pResult) = 0;
    };
} // namespace UN
