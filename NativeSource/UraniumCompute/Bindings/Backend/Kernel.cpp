#include <UnCompute/Backend/IKernel.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode IKernel_Init(IKernel* self, const KernelDesc& desc)
        {
            return self->Init(desc);
        }

        UN_DLL_EXPORT void IKernel_GetDesc(IKernel* self, KernelDesc& desc)
        {
            desc = self->GetDesc();
        }
    }
} // namespace UN
