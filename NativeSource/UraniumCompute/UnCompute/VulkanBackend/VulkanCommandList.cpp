#include <UnCompute/Backend/IFence.h>
#include <UnCompute/VulkanBackend/VulkanBuffer.h>
#include <UnCompute/VulkanBackend/VulkanCommandList.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanDeviceMemory.h>
#include <UnCompute/VulkanBackend/VulkanFence.h>
#include <UnCompute/VulkanBackend/VulkanKernel.h>
#include <UnCompute/VulkanBackend/VulkanResourceBinding.h>

namespace UN
{
    inline VkAccessFlags VulkanConvert(AccessFlags flags)
    {
        VkAccessFlags result = VK_FLAGS_NONE;
        // clang-format off
        if (AllFlagsActive(flags, AccessFlags::KernelRead))    { result |= VK_ACCESS_SHADER_READ_BIT;    }
        if (AllFlagsActive(flags, AccessFlags::KernelWrite))   { result |= VK_ACCESS_SHADER_WRITE_BIT;   }
        if (AllFlagsActive(flags, AccessFlags::TransferRead))  { result |= VK_ACCESS_TRANSFER_READ_BIT;  }
        if (AllFlagsActive(flags, AccessFlags::TransferWrite)) { result |= VK_ACCESS_TRANSFER_WRITE_BIT; }
        if (AllFlagsActive(flags, AccessFlags::HostRead))      { result |= VK_ACCESS_HOST_READ_BIT;      }
        if (AllFlagsActive(flags, AccessFlags::HostWrite))     { result |= VK_ACCESS_HOST_WRITE_BIT;     }
        // clang-format on
        return result;
    }

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

    void VulkanCommandList::CmdDispatch(IKernel* pKernel, Int32 x, Int32 y, Int32 z)
    {
        auto* pVkKernel        = un_verify_cast<VulkanKernel*>(pKernel);
        auto* pResourceBinding = pVkKernel->GetResourceBinding();
        auto descriptorSet     = pResourceBinding->GetNativeDescriptorSet();

        vkCmdBindPipeline(m_CommandBuffer, VK_PIPELINE_BIND_POINT_COMPUTE, pVkKernel->GetNativePipeline());
        vkCmdBindDescriptorSets(m_CommandBuffer,
                                VK_PIPELINE_BIND_POINT_COMPUTE,
                                pResourceBinding->GetNativePipelineLayout(),
                                0,
                                1,
                                &descriptorSet,
                                0,
                                nullptr);

        vkCmdDispatch(m_CommandBuffer, x, y, z);
    }

    VulkanCommandList::~VulkanCommandList()
    {
        Reset();
    }

    void VulkanCommandList::CmdMemoryBarrier(IBuffer* pBuffer, const MemoryBarrierDesc& barrierDesc)
    {
        VkBufferMemoryBarrier barrier{};
        barrier.sType         = VK_STRUCTURE_TYPE_BUFFER_MEMORY_BARRIER;
        barrier.offset        = 0;
        barrier.size          = VK_WHOLE_SIZE;
        barrier.buffer        = un_verify_cast<VulkanBuffer*>(pBuffer)->GetNativeBuffer();
        barrier.srcAccessMask = VulkanConvert(barrierDesc.SourceAccess);
        barrier.dstAccessMask = VulkanConvert(barrierDesc.DestAccess);

        auto* pDevice               = m_pDevice.As<VulkanComputeDevice>();
        barrier.srcQueueFamilyIndex = barrierDesc.SourceQueueKind == HardwareQueueKindFlags::None
            ? VK_QUEUE_FAMILY_IGNORED
            : pDevice->GetQueueFamilyIndex(barrierDesc.SourceQueueKind);
        barrier.dstQueueFamilyIndex = barrierDesc.DestQueueKind == HardwareQueueKindFlags::None
            ? VK_QUEUE_FAMILY_IGNORED
            : pDevice->GetQueueFamilyIndex(barrierDesc.DestQueueKind);

        vkCmdPipelineBarrier(m_CommandBuffer,
                             VK_PIPELINE_STAGE_ALL_COMMANDS_BIT,
                             VK_PIPELINE_STAGE_ALL_COMMANDS_BIT,
                             VK_FLAGS_NONE,
                             0,
                             nullptr,
                             1,
                             &barrier,
                             0,
                             nullptr);
    }
} // namespace UN
