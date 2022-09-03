#include <UnCompute/Acceleration/IDeviceFactory.h>
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Backend/IDeviceMemory.h>
#include <UnCompute/Memory/Memory.h>
#include <iostream>

using namespace UN;

int main()
{
    Ptr<DynamicLibrary> pLibrary;
    Ptr<IDeviceFactory> pFactory;

    CreateDeviceFactoryProc CreateDeviceFactory;
    UN_VerifyResultFatal(LoadCreateDeviceFactoryProc(&pLibrary, &CreateDeviceFactory), "Couldn't load DLL");
    UN_VerifyResultFatal(CreateDeviceFactory(BackendKind::Vulkan, &pFactory), "Couldn't create device factory");

    DeviceFactoryDesc deviceFactoryDesc("Test application");
    UN_VerifyResultFatal(pFactory->Init(deviceFactoryDesc), "Couldn't initialize device factory");

    auto adapters = pFactory->EnumerateAdapters();
    for (const AdapterInfo& adapter : adapters)
    {
        UNLOG_Info("Adapter #{}: {}", adapter.Id, adapter.Name);
    }

    Ptr<IComputeDevice> pDevice;
    UN_VerifyResultFatal(pFactory->CreateDevice(&pDevice), "Couldn't create device");

    ComputeDeviceDesc deviceDesc(adapters[0].Id);
    UN_VerifyResultFatal(pDevice->Init(deviceDesc), "Couldn't initialize device");

    Ptr<IBuffer> pBuffer;
    UN_VerifyResultFatal(pDevice->CreateBuffer(&pBuffer), "Couldn't create buffer");

    constexpr UInt64 bufferSize = 128 * 1024 * 1024;

    BufferDesc bufferDesc("Test buffer", bufferSize * sizeof(float));
    UN_VerifyResultFatal(pBuffer->Init(bufferDesc), "Couldn't initialize buffer");

    Ptr<IDeviceMemory> pMemory;
    UN_VerifyResultFatal(pDevice->CreateMemory(&pMemory), "Couldn't create device memory");

    // TODO: we need a nicer API for this...
    IDeviceObject* object = pBuffer.Get();
    auto buffers          = ArraySlice<const IDeviceObject* const>(&object, 1);
    auto memoryDesc = DeviceMemoryDesc("Test memory", MemoryKindFlags::HostAndDeviceAccessible, pBuffer->GetDesc().Size, buffers);

    UN_VerifyResultFatal(pMemory->Init(memoryDesc), "Couldn't allocate {} bytes of device memory", MemorySize64(memoryDesc.Size));

    UNLOG_Info("Allocated {} of device memory", MemorySize64(bufferSize * sizeof(float)));

    auto memorySlice = DeviceMemorySlice(pMemory.Get());
    UN_VerifyResultFatal(pBuffer->BindMemory(memorySlice), "Couldn't bind device memory to the buffer");

    if (auto data = MemoryMapHelper<float>::Map(memorySlice))
    {
        for (UInt64 i = 0; i < data.Length(); ++i)
        {
            data[i] = static_cast<float>(i);
        }

        std::cout << "Data in the buffer: ";
        for (UInt64 i = 0; i < 16; ++i)
        {
            std::cout << data[i] << " ";
        }

        std::cout << "..." << std::endl;
    }
    else
    {
        UN_Error(false, "Couldn't map memory");
    }
}
