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
        UN_UNUSED(desc);
        UNLOG_Debug("Creating Vulkan compute device on Adapter #{}", desc.AdapterId);
        return ResultCode::Success;
    }

    void VulkanComputeDevice::Reset()
    {
        ResetInternal();
    }

    ResultCode VulkanComputeDevice::Create(VulkanInstance* pInstance, VulkanComputeDevice** ppDevice)
    {
        *ppDevice = AllocateObject<VulkanComputeDevice>(pInstance);
        (*ppDevice)->AddRef();
        return ResultCode::Success;
    }

    VulkanComputeDevice::~VulkanComputeDevice()
    {
        ResetInternal();
    }

    void VulkanComputeDevice::ResetInternal()
    {
        UNLOG_Debug("Vulkan compute device was destroyed");
    }
} // namespace UN
