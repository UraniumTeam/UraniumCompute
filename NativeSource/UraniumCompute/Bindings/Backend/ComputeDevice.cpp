#include <UnCompute/Backend/IComputeDevice.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode IComputeDevice_Init(IComputeDevice* self, const ComputeDeviceDesc& desc)
        {
            return self->Init(desc);
        }

        UN_DLL_EXPORT ResultCode IComputeDevice_CreateBuffer(IComputeDevice* self, IBuffer** ppBuffer)
        {
            return self->CreateBuffer(ppBuffer);
        }

        UN_DLL_EXPORT ResultCode IComputeDevice_CreateMemory(IComputeDevice* self, IDeviceMemory** ppMemory)
        {
            return self->CreateMemory(ppMemory);
        }
    }
} // namespace UN
