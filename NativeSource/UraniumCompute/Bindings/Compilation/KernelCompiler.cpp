#include <UnCompute/Compilation/IKernelCompiler.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode IKernelCompiler_Init(IKernelCompiler* self, const KernelCompilerDesc& desc)
        {
            return self->Init(desc);
        }

        UN_DLL_EXPORT void IKernelCompiler_GetDesc(IKernelCompiler* self, KernelCompilerDesc& desc)
        {
            desc = self->GetDesc();
        }

        UN_DLL_EXPORT ResultCode IKernelCompiler_Compile(IKernelCompiler* self, const KernelCompilerArgs& args,
                                                         HeapArray<Byte>* pResult)
        {
            return self->Compile(args, pResult);
        }
    }
} // namespace UN
