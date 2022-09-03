#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanFence.h>

namespace UN
{
    VulkanFence::VulkanFence(IComputeDevice* pDevice)
        : FenceBase(pDevice)
    {
    }

    VulkanFence::~VulkanFence()
    {
        Reset();
    }

    void VulkanFence::Reset()
    {
        if (m_NativeFence == VK_NULL_HANDLE)
        {
            return;
        }

        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        vkDestroyFence(vkDevice, m_NativeFence, nullptr);
        m_NativeFence = VK_NULL_HANDLE;
    }

    ResultCode VulkanFence::SignalOnCpu()
    {
        auto queue = m_pDevice.As<VulkanComputeDevice>()->GetDeviceQueue(HardwareQueueKindFlags::Compute);

        VkSubmitInfo submitInfo{};
        submitInfo.sType = VK_STRUCTURE_TYPE_SUBMIT_INFO;

        auto vkResult = vkQueueSubmit(queue, 1, &submitInfo, m_NativeFence);
        UN_VerifyResult(vkResult, "Couldn't submit Vulkan queue to signal a fence");
        return VulkanConvert(vkResult);
    }

    ResultCode VulkanFence::WaitOnCpu(std::chrono::nanoseconds timeout)
    {
        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        auto vkResult = vkWaitForFences(vkDevice, 1, &m_NativeFence, false, timeout.count());
        return VulkanConvert(vkResult);
    }

    void VulkanFence::ResetState()
    {
        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        vkResetFences(vkDevice, 1, &m_NativeFence);
    }

    FenceState VulkanFence::GetState()
    {
        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        auto status   = vkGetFenceStatus(vkDevice, m_NativeFence);
        return status == VK_SUCCESS ? FenceState::Signaled : FenceState::Reset;
    }

    ResultCode VulkanFence::InitInternal(const DescriptorType& desc)
    {
        VkFenceCreateInfo fenceCI{};
        fenceCI.sType = VK_STRUCTURE_TYPE_FENCE_CREATE_INFO;
        fenceCI.flags = desc.InitialState == FenceState::Reset ? 0 : VK_FENCE_CREATE_SIGNALED_BIT;

        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        auto result   = vkCreateFence(vkDevice, &fenceCI, VK_NULL_HANDLE, &m_NativeFence);
        UN_Error(Succeeded(result), "Couldn't initialize Vulkan fence, vkCreateFence returned {}", result);
        return VulkanConvert(result);
    }
} // namespace UN
