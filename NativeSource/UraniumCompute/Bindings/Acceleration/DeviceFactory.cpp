#include <UnCompute/Acceleration/IDeviceFactory.h>
#include <UnCompute/Containers/HeapArray.h>

#define SELF un_verify_cast<IDeviceFactory*>(self)

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode IDeviceFactory_Init(IObject* self, const DeviceFactoryDesc& desc)
        {
            return SELF->Init(desc);
        }

        UN_DLL_EXPORT void IDeviceFactory_Reset(IObject* self)
        {
            SELF->Reset();
        }

        UN_DLL_EXPORT BackendKind IDeviceFactory_GetBackendKind(IObject* self)
        {
            return SELF->GetBackendKind();
        }

        UN_DLL_EXPORT void IDeviceFactory_EnumerateAdapters(IObject* self, HeapArray<AdapterInfo>* pAdapters)
        {
            auto adapters = SELF->EnumerateAdapters();
            new (pAdapters) HeapArray<AdapterInfo>(adapters.Length());
            adapters.CopyDataTo(*pAdapters);
        }

        UN_DLL_EXPORT ResultCode IDeviceFactory_CreateDevice(IObject* self, IComputeDevice** ppDevice)
        {
            return SELF->CreateDevice(ppDevice);
        }
    }
} // namespace UN
