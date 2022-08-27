#include <UnCompute/Backend/IBuffer.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode IBuffer_Init(IBuffer* self, const BufferDesc& desc)
        {
            return self->Init(desc);
        }

        UN_DLL_EXPORT void IBuffer_GetDesc(IBuffer* self, BufferDesc& desc)
        {
            desc = self->GetDesc();
        }

        UN_DLL_EXPORT ResultCode IBuffer_BindMemory(IBuffer* self, const DeviceMemorySlice& slice)
        {
            return self->BindMemory(slice);
        }
    }
} // namespace UN
