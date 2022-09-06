#include <UnCompute/Backend/IFence.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode IFence_Init(IFence* self, const FenceDesc& desc)
        {
            return self->Init(desc);
        }

        UN_DLL_EXPORT void IFence_GetDesc(IFence* self, FenceDesc& desc)
        {
            desc = self->GetDesc();
        }

        UN_DLL_EXPORT ResultCode IFence_SignalOnCpu(IFence* self)
        {
            return self->SignalOnCpu();
        }

        UN_DLL_EXPORT ResultCode IFence_WaitOnCpu_Timeout(IFence* self, UInt64 timeout)
        {
            return self->WaitOnCpu(std::chrono::nanoseconds(timeout));
        }

        UN_DLL_EXPORT ResultCode IFence_WaitOnCpu(IFence* self)
        {
            return self->WaitOnCpu();
        }

        UN_DLL_EXPORT void IFence_ResetState(IFence* self)
        {
            self->ResetState();
        }

        UN_DLL_EXPORT FenceState IFence_GetState(IFence* self)
        {
            return self->GetState();
        }
    }
} // namespace UN
