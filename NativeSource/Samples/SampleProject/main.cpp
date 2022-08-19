#include <UnCompute/Acceleration/DeviceFactory.h>
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Memory/Memory.h>

using namespace UN;

int main()
{
    InitializeLogger();
    Ptr<DeviceFactory> pFactory;
    UN_VerifyResult(DeviceFactory::Create(&pFactory), "Couldn't create Vulkan factory");
    UN_VerifyResult(pFactory->Init(BackendKind::Vulkan), "Couldn't initialize Vulkan factory");

    auto adapters = pFactory->EnumerateAdapters();
    Ptr<IComputeDevice> pDevice;
    UN_VerifyResult(pFactory->CreateDevice(&pDevice), "Couldn't create Vulkan device");
    UN_VerifyResult(pDevice->Init(ComputeDeviceDesc(adapters[0].Id)), "Couldn't initialize Vulkan device");
}
