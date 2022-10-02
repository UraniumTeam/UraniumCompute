#include <UnCompute/Backend/IFence.h>
#include <UnCompute/VulkanBackend/VulkanBuffer.h>
#include <UnCompute/VulkanBackend/VulkanCommandList.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanDeviceMemory.h>
#include <UnCompute/VulkanBackend/VulkanFence.h>

namespace UN
{
    VulkanCommandList::VulkanCommandList(IComputeDevice* pDevice)
        : CommandListBase(pDevice)
    {
    }

    CommandListState VulkanCommandList::GetState()
    {
        if (m_State == CommandListState::Pending)
        {
            if (m_pFence->GetState() == FenceState::Signaled)
            {
                m_State = AnyFlagsActive(m_Desc.Flags, CommandListFlags::OneTimeSubmit) ? CommandListState::Invalid
                                                                                        : CommandListState::Executable;
            }
        }

        return m_State;
    }

    void VulkanCommandList::Reset()
    {
        if (m_CommandBuffer == VK_NULL_HANDLE)
        {
            return;
        }

        auto device = m_pDevice.As<VulkanComputeDevice>();
        vkFreeCommandBuffers(device->GetNativeDevice(), m_CommandPool, 1, &m_CommandBuffer);
        m_CommandBuffer = VK_NULL_HANDLE;
        m_CommandPool   = VK_NULL_HANDLE;
        m_Queue         = VK_NULL_HANDLE;
    }

    ResultCode VulkanCommandList::InitInternal(const CommandListDesc& desc)
    {
        if (auto result = m_pDevice->CreateFence(&m_pFence); Failed(result))
        {
            UN_VerifyError(false, "Couldn't create a fence for command list");
            return result;
        }
        if (auto result = m_pFence->Init(FenceDesc("Command list wait fence")); Failed(result))
        {
            UN_VerifyError(false, "Couldn't initialize a fence for command list");
            return result;
        }

        auto device           = m_pDevice.As<VulkanComputeDevice>();
        auto queueFamilyIndex = device->GetQueueFamilyIndex(desc.QueueKindFlags);
        m_CommandPool         = device->GetCommandPool(queueFamilyIndex);
        m_Queue               = device->GetDeviceQueue(queueFamilyIndex, 0);

        VkCommandBufferAllocateInfo allocateInfo{};
        allocateInfo.sType              = VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO;
        allocateInfo.commandPool        = m_CommandPool;
        allocateInfo.commandBufferCount = 1;
        allocateInfo.level              = VK_COMMAND_BUFFER_LEVEL_PRIMARY;

        auto vkResult = vkAllocateCommandBuffers(device->GetNativeDevice(), &allocateInfo, &m_CommandBuffer);
        UN_VerifyResult(vkResult, "Couldn't allocate Vulkan command buffer");
        return VulkanConvert(vkResult);
    }

    ResultCode VulkanCommandList::BeginInternal()
    {
        VkCommandBufferBeginInfo beginInfo{};
        beginInfo.sType = VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO;
        if (AnyFlagsActive(m_Desc.Flags, CommandListFlags::OneTimeSubmit))
        {
            beginInfo.flags = VK_COMMAND_BUFFER_USAGE_ONE_TIME_SUBMIT_BIT;
        }

        return VulkanConvert(vkBeginCommandBuffer(m_CommandBuffer, &beginInfo));
    }

    ResultCode VulkanCommandList::EndInternal()
    {
        return VulkanConvert(vkEndCommandBuffer(m_CommandBuffer));
    }

    ResultCode VulkanCommandList::ResetStateInternal()
    {
        return VulkanConvert(vkResetCommandBuffer(m_CommandBuffer, VK_FLAGS_NONE));
    }

    ResultCode VulkanCommandList::SubmitInternal()
    {
        VkSubmitInfo info{};
        info.sType              = VK_STRUCTURE_TYPE_SUBMIT_INFO;
        info.pCommandBuffers    = &m_CommandBuffer;
        info.commandBufferCount = 1;

        m_pFence->ResetState();
        VkFence vkFence = m_pFence.As<VulkanFence>()->GetNativeFence();
        return VulkanConvert(vkQueueSubmit(m_Queue, 1, &info, vkFence));
    }

    void VulkanCommandList::CmdCopy(IBuffer* pSource, IBuffer* pDestination, const BufferCopyRegion& region)
    {
        auto nativeSrc = un_verify_cast<VulkanBuffer*>(pSource)->GetNativeBuffer();
        auto nativeDst = un_verify_cast<VulkanBuffer*>(pDestination)->GetNativeBuffer();

        VkBufferCopy copy{};
        copy.size      = region.Size;
        copy.dstOffset = region.DestOffset;
        copy.srcOffset = region.SourceOffset;
        vkCmdCopyBuffer(m_CommandBuffer, nativeSrc, nativeDst, 1, &copy);
    }

    VulkanCommandList::~VulkanCommandList()
    {
        Reset();
    }
} // namespace UN
