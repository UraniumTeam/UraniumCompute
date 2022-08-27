#include <UnCompute/Backend/IDeviceObject.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT void IDeviceObject_Reset(IDeviceObject* self)
        {
            self->Reset();
        }

        UN_DLL_EXPORT IComputeDevice* IDeviceObject_GetDevice(IDeviceObject* self)
        {
            return self->GetDevice();
        }

        UN_DLL_EXPORT const char* IDeviceObject_GetDebugName(IDeviceObject* self)
        {
            return self->GetDebugName().data();
        }
    }
} // namespace UN
