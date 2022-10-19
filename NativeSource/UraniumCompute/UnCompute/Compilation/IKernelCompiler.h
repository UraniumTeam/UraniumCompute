#pragma once
#include <UnCompute/Base/Byte.h>
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

        inline KernelCompilerDesc() = default;

        inline explicit KernelCompilerDesc(const char* name, KernelSourceLang sourceLang = KernelSourceLang::Hlsl,
                                           KernelTargetLang targetLang = KernelTargetLang::SpirV)
            : Name(name)
            , SourceLang(sourceLang)
            , TargetLang(targetLang)
        {
        }
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

    //! Compiler `#define` descriptor.
    struct CompilerDefinition
    {
        const char* Name  = nullptr; //!< Name in `#define NAME VALUE`.
        const char* Value = nullptr; //!< Value in `#define NAME VALUE`.

        CompilerDefinition() = default;

        inline explicit CompilerDefinition(const char* name, const char* value = nullptr)
            : Name(name)
            , Value(value)
        {
        }
    };

    //! \brief Kernel compiler arguments that define a single compilation.
    struct KernelCompilerArgs
    {
        ArraySlice<const Byte> SourceCode; //!< Compute shader source code in a high-level language, e.g. HLSL.
        CompilerOptimizationLevel OptimizationLevel = CompilerOptimizationLevel::Max; //!< Compiler optimization level.
        const char* EntryPoint                      = "main";                         //!< Compute shader entry point.
        ArraySlice<CompilerDefinition> Definitions;                                   //!< Compiler definitions.
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
        virtual ResultCode Compile(const KernelCompilerArgs& args, HeapArray<Byte>* pResult) = 0;
    };
} // namespace UN
