#pragma once
#include <UnCompute/Backend/CommandListBase.h>
#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>

namespace UN
{
    class VulkanCommandList final : public CommandListBase
    {
        VkCommandBuffer m_CommandBuffer = VK_NULL_HANDLE;
        VkCommandPool m_CommandPool     = VK_NULL_HANDLE;
        VkQueue m_Queue                 = VK_NULL_HANDLE;

    protected:
        ResultCode InitInternal(const CommandListDesc& desc) override;
        ResultCode BeginInternal() override;
        ResultCode EndInternal() override;
        ResultCode ResetStateInternal() override;
        ResultCode SubmitInternal() override;

        void CmdMemoryBarrier(IBuffer* pBuffer, const MemoryBarrierDesc& barrierDesc) override;
        void CmdCopy(IBuffer* pSource, IBuffer* pDestination, const BufferCopyRegion& region) override;
        void CmdDispatch(IKernel* pKernel, Int32 x, Int32 y, Int32 z) override;

    public:
        explicit VulkanCommandList(IComputeDevice* pDevice);
        ~VulkanCommandList() override;

        CommandListState GetState() override;
        void Reset() override;

        inline static ResultCode Create(IComputeDevice* pDevice, ICommandList** ppCommandList)
        {
            *ppCommandList = AllocateObject<VulkanCommandList>(pDevice);
            (*ppCommandList)->AddRef();
            return ResultCode::Success;
        }
    };
} // namespace UN
