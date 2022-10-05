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

        UN_DLL_EXPORT ResultCode IComputeDevice_CreateFence(IComputeDevice* self, IFence** ppFence)
        {
            return self->CreateFence(ppFence);
        }

        UN_DLL_EXPORT ResultCode IComputeDevice_CreateCommandList(IComputeDevice* self, ICommandList** ppCommandList)
        {
            return self->CreateCommandList(ppCommandList);
        }

        UN_DLL_EXPORT ResultCode IComputeDevice_CreateResourceBinding(IComputeDevice* self, IResourceBinding** ppResourceBinding)
        {
            return self->CreateResourceBinding(ppResourceBinding);
        }

        UN_DLL_EXPORT ResultCode IComputeDevice_CreateKernel(IComputeDevice* self, IKernel** ppKernel)
        {
            return self->CreateKernel(ppKernel);
        }
    }
} // namespace UN
