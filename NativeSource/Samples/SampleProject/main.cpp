#include <UnCompute/Acceleration/IDeviceFactory.h>
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Backend/IDeviceMemory.h>
#include <UnCompute/Memory/Memory.h>

using namespace UN;

int main()
{
    Ptr<DynamicLibrary> pLibrary;
    Ptr<IDeviceFactory> pFactory;

    CreateDeviceFactoryProc CreateDeviceFactory;
    UN_VerifyResult(LoadCreateDeviceFactoryProc(&pLibrary, &CreateDeviceFactory), "Couldn't load DLL");
    UN_VerifyResult(CreateDeviceFactory(BackendKind::Vulkan, &pFactory), "Couldn't create device factory");

    DeviceFactoryDesc deviceFactoryDesc("Test application");
    UN_VerifyResult(pFactory->Init(deviceFactoryDesc), "Couldn't initialize device factory");

    auto adapters = pFactory->EnumerateAdapters();
    for (const AdapterInfo& adapter : adapters)
    {
        UNLOG_Info("Adapter #{}: {}", adapter.Id, adapter.Name);
    }

    Ptr<IComputeDevice> pDevice;
    UN_VerifyResult(pFactory->CreateDevice(&pDevice), "Couldn't create device");

    ComputeDeviceDesc deviceDesc(adapters[0].Id);
    UN_VerifyResult(pDevice->Init(deviceDesc), "Couldn't initialize device");

    Ptr<IBuffer> pBuffer;
    UN_VerifyResult(pDevice->CreateBuffer(&pBuffer), "Couldn't create buffer");

    BufferDesc bufferDesc("Test buffer", 1024);
    UN_VerifyResult(pBuffer->Init(bufferDesc), "Couldn't initialize buffer");
}
