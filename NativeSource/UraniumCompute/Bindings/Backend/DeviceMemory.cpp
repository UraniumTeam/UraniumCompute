#include <UnCompute/Backend/IDeviceMemory.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode IDeviceMemory_Init(IDeviceMemory* self, const DeviceMemoryDesc& desc)
        {
            return self->Init(desc);
        }

        UN_DLL_EXPORT void IDeviceMemory_GetDesc(IDeviceMemory* self, DeviceMemoryDesc& desc)
        {
            desc = self->GetDesc();
        }

        UN_DLL_EXPORT ResultCode IDeviceMemory_Map(IDeviceMemory* self, UInt64 byteOffset, UInt64 byteSize, void** ppData)
        {
            return self->Map(byteOffset, byteSize, ppData);
        }

        UN_DLL_EXPORT void IDeviceMemory_Unmap(IDeviceMemory* self)
        {
            return self->Unmap();
        }

        UN_DLL_EXPORT bool IDeviceMemory_IsCompatible(IDeviceMemory* self, IDeviceObject* pObject)
        {
            return self->IsCompatible(pObject);
        }
    }
} // namespace UN
