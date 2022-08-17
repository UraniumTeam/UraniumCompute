#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanInstance.h>

namespace UN
{
    VulkanComputeDevice::VulkanComputeDevice(VulkanInstance* pInstance)
    {
        m_Instance = pInstance;
    }

    void VulkanComputeDevice::Init(const ComputeDeviceDesc& desc)
    {
        (void)desc;
    }

    void VulkanComputeDevice::Reset() {}
} // namespace UN
