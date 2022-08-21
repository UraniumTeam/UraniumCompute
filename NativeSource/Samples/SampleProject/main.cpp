#include <UnCompute/Acceleration/IDeviceFactory.h>
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Memory/Memory.h>

using namespace UN;

int main()
{
    Ptr<DynamicLibrary> pLibrary;
    Ptr<IDeviceFactory> pFactory;

    CreateDeviceFactoryProc CreateDeviceFactory;
    UN_VerifyResult(LoadCreateDeviceFactoryProc(&pLibrary, &CreateDeviceFactory), "Couldn't load DLL");
    UN_VerifyResult(CreateDeviceFactory(BackendKind::Vulkan, &pFactory), "Couldn't create Vulkan factory");

    DeviceFactoryDesc deviceFactoryDesc("Test application");
    UN_VerifyResult(pFactory->Init(deviceFactoryDesc), "Couldn't initialize Vulkan factory");

    auto adapters = pFactory->EnumerateAdapters();
    for (const AdapterInfo& adapter : adapters)
    {
        UNLOG_Info("Adapter #{}: {}", adapter.Id, adapter.Name);
    }

    Ptr<IComputeDevice> pDevice;
    UN_VerifyResult(pFactory->CreateDevice(&pDevice), "Couldn't create Vulkan device");

    ComputeDeviceDesc deviceDesc(adapters[0].Id);
    UN_VerifyResult(pDevice->Init(deviceDesc), "Couldn't initialize Vulkan device");
}
