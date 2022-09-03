#pragma once
#include <UnCompute/Backend/FenceBase.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>

namespace UN
{
    class VulkanFence final : public FenceBase
    {
        VkFence m_NativeFence = VK_NULL_HANDLE;

    protected:
        ResultCode InitInternal(const DescriptorType& desc) override;

    public:
        explicit VulkanFence(IComputeDevice* pDevice);
        ~VulkanFence() override;

        void Reset() override;
        ResultCode SignalOnCpu() override;
        ResultCode WaitOnCpu() override;
        void ResetState() override;
        FenceState GetState() override;

        [[nodiscard]] inline VkFence GetNativeFence() const
        {
            return m_NativeFence;
        }
    };
} // namespace UN
