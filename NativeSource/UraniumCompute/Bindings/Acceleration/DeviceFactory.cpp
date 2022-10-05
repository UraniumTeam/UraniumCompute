#include <UnCompute/Acceleration/IDeviceFactory.h>
#include <UnCompute/Containers/HeapArray.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode IDeviceFactory_Init(IDeviceFactory* self, const DeviceFactoryDesc& desc)
        {
            return self->Init(desc);
        }

        UN_DLL_EXPORT void IDeviceFactory_EnumerateAdapters(IDeviceFactory* self, HeapArray<AdapterInfo>* pAdapters)
        {
            auto adapters = self->EnumerateAdapters();
            new (pAdapters) HeapArray<AdapterInfo>(adapters.Length());
            adapters.CopyDataTo(*pAdapters);
        }

        UN_DLL_EXPORT ResultCode IDeviceFactory_CreateDevice(IDeviceFactory* self, IComputeDevice** ppDevice)
        {
            return self->CreateDevice(ppDevice);
        }

        UN_DLL_EXPORT ResultCode IDeviceFactory_CreateKernelCompiler(IDeviceFactory* self, IKernelCompiler** ppCompiler)
        {
            return self->CreateKernelCompiler(ppCompiler);
        }
    }
} // namespace UN
