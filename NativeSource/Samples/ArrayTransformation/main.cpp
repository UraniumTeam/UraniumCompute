#include <UnCompute/Acceleration/IDeviceFactory.h>
#include <UnCompute/Backend/ICommandList.h>
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Backend/IDeviceMemory.h>
#include <UnCompute/Backend/IFence.h>
#include <UnCompute/Backend/IKernel.h>
#include <UnCompute/Backend/IResourceBinding.h>
#include <UnCompute/Compilation/IKernelCompiler.h>
#include <UnCompute/Memory/Memory.h>
#include <UnCompute/Utils/MemoryUtils.h>
#include <algorithm>
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

    Ptr<IComputeDevice> pDevice;
    UN_VerifyResultFatal(pFactory->CreateDevice(&pDevice), "Couldn't create device");

    auto adapters = pFactory->EnumerateAdapters();

    auto* pGpu = std::find_if(adapters.begin(), adapters.end(), [](const AdapterInfo& info) {
        return info.Kind == AdapterKind::Discrete;
    });

    if (pGpu == adapters.end())
    {
        pGpu = adapters.Data();
    }

    ComputeDeviceDesc deviceDesc(pGpu->Id);
    UN_VerifyResultFatal(pDevice->Init(deviceDesc), "Couldn't initialize device");

    Ptr<IBuffer> pStagingBuffer, pDeviceBuffer, pConstantBuffer;
    UN_VerifyResultFatal(pDevice->CreateBuffer(&pStagingBuffer), "Couldn't create buffer");
    UN_VerifyResultFatal(pDevice->CreateBuffer(&pDeviceBuffer), "Couldn't create buffer");
    UN_VerifyResultFatal(pDevice->CreateBuffer(&pConstantBuffer), "Couldn't create buffer");

    using BufferType = UInt32;

    constexpr UInt64 workgroupSize      = 16;
    constexpr UInt64 bufferElementCount = 32;
    constexpr UInt64 bufferSize         = bufferElementCount * sizeof(BufferType);
    UN_VerifyResultFatal(pStagingBuffer->Init(BufferDesc("Staging buffer", bufferSize)), "Couldn't initialize buffer");
    UN_VerifyResultFatal(pDeviceBuffer->Init(BufferDesc("Device local buffer", bufferSize)), "Couldn't initialize buffer");
    UN_VerifyResultFatal(pConstantBuffer->Init(BufferDesc("Constant buffer", sizeof(BufferType), BufferUsage::Constant)),
                         "Couldn't initialize buffer");

    UNLOG_Info("Allocating 2x {} of memory", MemorySize(bufferSize));
    Ptr<IDeviceMemory> pStagingMemory, pDeviceMemory, pConstantMemory;
    UN_VerifyResultFatal(
        Utility::AllocateMemoryFor(pStagingBuffer.Get(), MemoryKindFlags::HostAndDeviceAccessible, &pStagingMemory),
        "Couldn't allocate staging memory");
    UN_VerifyResultFatal( //
        Utility::AllocateMemoryFor(pStagingBuffer.Get(), MemoryKindFlags::DeviceAccessible, &pDeviceMemory),
        "Couldn't allocate device local memory");
    UN_VerifyResultFatal(
        Utility::AllocateMemoryFor(pConstantBuffer.Get(), MemoryKindFlags::HostAndDeviceAccessible, &pConstantMemory),
        "Couldn't allocate constant memory");

    UN_VerifyResultFatal(pStagingBuffer->BindMemory(pStagingMemory.Get()), "Couldn't bind memory to the staging buffer");
    UN_VerifyResultFatal(pDeviceBuffer->BindMemory(pDeviceMemory.Get()), "Couldn't bind memory to the device buffer");
    UN_VerifyResultFatal(pConstantBuffer->BindMemory(pConstantMemory.Get()), "Couldn't bind memory to the device buffer");

    if (auto map = MemoryMapHelper<BufferType>::Map(pConstantMemory.Get()))
    {
        map[0] = 10;
    }
    else
    {
        UN_Error(false, "Couldn't map constant memory");
    }

    if (auto map = MemoryMapHelper<BufferType>::Map(pStagingMemory.Get()))
    {
        BufferType n = 0;
        std::generate(map.begin(), map.end(), [&n] {
            return n++;
        });
    }
    else
    {
        UN_Error(false, "Couldn't map staging memory");
    }

    Ptr<ICommandList> pCommandList;
    UN_VerifyResultFatal(pDevice->CreateCommandList(&pCommandList), "Couldn't create command list");

    CommandListDesc commandListDesc("Command list", HardwareQueueKindFlags::Compute);
    UN_VerifyResultFatal(pCommandList->Init(commandListDesc), "Couldn't initialize command list");

    if (auto builder = pCommandList->Begin())
    {
        BufferCopyRegion copyRegion(bufferSize);
        builder.Copy(pStagingBuffer.Get(), pDeviceBuffer.Get(), copyRegion);
    }
    else
    {
        UN_Error(false, "Couldn't begin command list recording");
    }

    using namespace std::chrono_literals;

    // Can also be synchronized with memory barrier commands in this case (should be faster)
    auto pFence = pCommandList->GetFence();
    UNLOG_Info("Waiting for copy command lists");
    UN_VerifyResultFatal(pCommandList->Submit(), "Couldn't submit GPU commands");
    UN_VerifyResultFatal(pFence->WaitOnCpu(1s), "Command list fence timeout");

    UNLOG_Info("Copy complete");

    Ptr<IKernelCompiler> pKernelCompiler;
    UN_VerifyResultFatal(pFactory->CreateKernelCompiler(&pKernelCompiler), "Couldn't create kernel compiler");

    KernelCompilerDesc compilerDesc("Kernel compiler");
    UN_VerifyResultFatal(pKernelCompiler->Init(compilerDesc), "Couldn't initialize kernel compiler");

    std::string kernelSource = R"(
RWStructuredBuffer<uint> values : register(u0);

cbuffer Constants : register(b1)
{
    uint Offset;
}

uint fib(uint n)
{
    n %= 16;
    if(n <= 1) return n;

    uint c = 1;
    uint p = 1;

    for (uint i = 2; i < n; ++i)
    {
        uint t = c;
        c += p;
        p = t;
    }

    return c;
}

[numthreads(1, 1, 1)]
void main(uint3 globalInvocationID : SV_DispatchThreadID)
{
    uint index = globalInvocationID.x;
    for (uint i = index * WORKGROUP_SIZE; i < (index + 1) * WORKGROUP_SIZE; ++i)
        values[i] = fib(values[i]) + Offset;
}
)";

    auto workgroupSizeStr = std::to_string(workgroupSize);

    CompilerDefinition definitions[] = { CompilerDefinition("WORKGROUP_SIZE", workgroupSizeStr.c_str()),
                                         CompilerDefinition("TEST_DEF", "123") };

    KernelCompilerArgs compilerArgs;
    compilerArgs.SourceCode =
        ArraySlice(un_byte_cast(kernelSource.c_str()), un_byte_cast(kernelSource.c_str() + kernelSource.size()));
    compilerArgs.Definitions = definitions;

    HeapArray<Byte> bytecode;
    UN_VerifyResultFatal(pKernelCompiler->Compile(compilerArgs, &bytecode), "Couldn't compile compute kernel");

    Ptr<IResourceBinding> pResourceBinding;
    UN_VerifyResultFatal(pDevice->CreateResourceBinding(&pResourceBinding), "Couldn't create resource binding");

    KernelResourceDesc bindingLayout[] = { KernelResourceDesc(0, KernelResourceKind::RWBuffer),
                                           KernelResourceDesc(1, KernelResourceKind::ConstantBuffer) };
    ResourceBindingDesc resourceBindingDesc("Resource binding", bindingLayout);
    UN_VerifyResultFatal(pResourceBinding->Init(resourceBindingDesc), "Couldn't initialize resource binding");
    UN_VerifyResultFatal(pResourceBinding->SetVariable(0, pDeviceBuffer.Get()), "Couldn't set buffer variable");
    UN_VerifyResultFatal(pResourceBinding->SetVariable(1, pConstantBuffer.Get()), "Couldn't set buffer variable");

    Ptr<IKernel> pKernel;
    UN_VerifyResultFatal(pDevice->CreateKernel(&pKernel), "Couldn't create compute kernel");

    KernelDesc kernelDesc("Compute kernel", pResourceBinding.Get(), bytecode);
    UN_VerifyResultFatal(pKernel->Init(kernelDesc), "Couldn't initialize  compute kernel");

    pCommandList->ResetState();
    if (auto builder = pCommandList->Begin())
    {
        builder.Dispatch(pKernel.Get(), bufferElementCount / workgroupSize, 1, 1);
    }
    else
    {
        UN_Error(false, "Couldn't run kernel");
    }

    UNLOG_Info("Waiting for dispatch command lists");
    UN_VerifyResultFatal(pCommandList->Submit(), "Couldn't submit GPU commands");
    UN_VerifyResultFatal(pFence->WaitOnCpu(), "Command list fence timeout");
    UNLOG_Info("Dispatch complete");
    pCommandList->ResetState();

    if (auto builder = pCommandList->Begin())
    {
        BufferCopyRegion copyRegion(bufferSize);
        builder.Copy(pDeviceBuffer.Get(), pStagingBuffer.Get(), copyRegion);
    }
    else
    {
        UN_Error(false, "Couldn't begin command list recording");
    }

    UNLOG_Info("Waiting for copy command lists");
    UN_VerifyResultFatal(pCommandList->Submit(), "Couldn't submit GPU commands");
    UN_VerifyResultFatal(pFence->WaitOnCpu(), "Command list fence timeout");
    UNLOG_Info("Copy complete");

    if (auto data = MemoryMapHelper<BufferType>::Map(pStagingMemory.Get()))
    {
        std::cout << "Calculation results: ";
        for (UInt64 i = 0; i < 32; ++i)
        {
            std::cout << data[i] << " ";
        }

        if (bufferElementCount > 32) // NOLINT
        {
            std::cout << "...";
        }

        std::cout << std::endl;
    }
    else
    {
        UN_Error(false, "Couldn't map memory");
    }
}
