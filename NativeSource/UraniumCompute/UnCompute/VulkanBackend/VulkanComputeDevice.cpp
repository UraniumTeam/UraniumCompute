#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanInstance.h>

namespace UN
{
    VulkanComputeDevice::VulkanComputeDevice(VulkanInstance* pInstance)
        : m_pInstance(pInstance)
    {
    }

    ResultCode VulkanComputeDevice::Init(const ComputeDeviceDesc& desc)
    {
        (void)desc;
        return ResultCode::Success;
    }

    void VulkanComputeDevice::Reset() {}

    ResultCode VulkanComputeDevice::Create(VulkanInstance* pInstance, VulkanComputeDevice** ppDevice)
    {
        *ppDevice = AllocateObject<VulkanComputeDevice>(pInstance);
        (*ppDevice)->AddRef();
        return ResultCode::Success;
    }
} // namespace UN
