#include <UnCompute/Acceleration/IDeviceFactory.h>
#include <UnCompute/Backend/ICommandList.h>
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Backend/IDeviceMemory.h>
#include <UnCompute/Backend/IFence.h>
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

    Ptr<IBuffer> pBuffer1, pBuffer2;
    UN_VerifyResultFatal(pDevice->CreateBuffer(&pBuffer1), "Couldn't create buffer");
    UN_VerifyResultFatal(pDevice->CreateBuffer(&pBuffer2), "Couldn't create buffer");

    constexpr UInt64 bufferElementCount = 1024 * 1024;
    constexpr UInt64 bufferSize         = bufferElementCount * sizeof(float);
    constexpr UInt64 memorySize         = bufferSize * 2;

    BufferDesc bufferDesc("Test buffer", bufferSize);
    UN_VerifyResultFatal(pBuffer1->Init(bufferDesc), "Couldn't initialize buffer");
    UN_VerifyResultFatal(pBuffer2->Init(bufferDesc), "Couldn't initialize buffer");

    Ptr<IDeviceMemory> pMemory;
    UN_VerifyResultFatal(pDevice->CreateMemory(&pMemory), "Couldn't create device memory");

    // TODO: we need a nicer API for this...
    IDeviceObject* objects[] = { pBuffer1.Get(), pBuffer2.Get() };
    auto memoryDesc          = DeviceMemoryDesc(
        "Test memory", MemoryKindFlags::HostAndDeviceAccessible, memorySize, ArraySlice<IDeviceObject*>(objects));

    UN_VerifyResultFatal(pMemory->Init(memoryDesc), "Couldn't allocate {} bytes of device memory", MemorySize64(memoryDesc.Size));

    UNLOG_Info("Allocated {} of device memory", MemorySize64(memorySize));

    auto memorySlice1 = DeviceMemorySlice(pMemory.Get(), 0, bufferSize);
    UN_VerifyResultFatal(pBuffer1->BindMemory(memorySlice1), "Couldn't bind device memory to the buffer");

    auto memorySlice2 = DeviceMemorySlice(pMemory.Get(), bufferSize, bufferSize);
    UN_VerifyResultFatal(pBuffer2->BindMemory(memorySlice2), "Couldn't bind device memory to the buffer");

    if (auto data = MemoryMapHelper<float>::Map(memorySlice1))
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

    using namespace std::chrono_literals;

    Ptr<IFence> pFence;
    UN_VerifyResultFatal(pDevice->CreateFence(&pFence), "Couldn't create a fence");

    FenceDesc fenceDesc("Test fence");
    UN_VerifyResultFatal(pFence->Init(fenceDesc), "Couldn't initialize a fence");
    pFence->SignalOnCpu();
    UNLOG_Info("WaitOnCpu() for \"{}\" returned {}", pFence->GetDebugName(), pFence->WaitOnCpu(10ms));
    pFence->ResetState();
    UNLOG_Info("WaitOnCpu() for \"{}\" returned {}", pFence->GetDebugName(), pFence->WaitOnCpu(10ms));
    pFence->SignalOnCpu();
    UNLOG_Info("WaitOnCpu() for \"{}\" returned {}", pFence->GetDebugName(), pFence->WaitOnCpu(10ms));

    Ptr<ICommandList> pCommandList;
    UN_VerifyResultFatal(pDevice->CreateCommandList(&pCommandList), "Couldn't create a command list");

    UNLOG_Info(pCommandList->GetState());

    CommandListDesc commandListDesc("Test command list", HardwareQueueKindFlags::Compute);
    UN_VerifyResultFatal(pCommandList->Init(commandListDesc), "Couldn't initialize a command list");

    UNLOG_Info(pCommandList->GetState());

    if (auto builder = pCommandList->Begin())
    {
        UNLOG_Info(pCommandList->GetState());
        BufferCopyRegion copyRegion(bufferSize);
        builder.Copy(pBuffer1.Get(), pBuffer2.Get(), copyRegion);
    }
    else
    {
        UN_Error(false, "Couldn't begin command list recording");
    }

    UNLOG_Info(pCommandList->GetState());

    if (auto data = MemoryMapHelper<float>::Map(memorySlice2))
    {
        std::cout << "Data in the buffer before copy: ";
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

    UN_VerifyResultFatal(pCommandList->Submit(), "Couldn't submit GPU commands");
    UNLOG_Info(pCommandList->GetState());
    Ptr<IFence> pWaitFence = pCommandList->GetFence();
    UN_VerifyResultFatal(pWaitFence->WaitOnCpu(1s), "Command list fence timeout");
    UNLOG_Info(pCommandList->GetState());

    if (auto data = MemoryMapHelper<float>::Map(memorySlice2))
    {
        std::cout << "Data in the copied buffer: ";
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
