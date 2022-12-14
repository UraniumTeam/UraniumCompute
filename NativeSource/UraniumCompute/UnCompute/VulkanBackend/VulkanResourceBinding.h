#pragma once
#include <UnCompute/Backend/ResourceBindingBase.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>

namespace UN
{
    class VulkanResourceBinding final : public ResourceBindingBase
    {
        VkDescriptorSetLayout m_SetLayout = VK_NULL_HANDLE;
        VkDescriptorSet m_DescriptorSet   = VK_NULL_HANDLE;

        VkPipelineLayout m_PipelineLayout = VK_NULL_HANDLE;

    protected:
        ResultCode InitInternal(const DescriptorType& desc) override;

    public:
        explicit VulkanResourceBinding(IComputeDevice* pDevice);
        ~VulkanResourceBinding() override;

        ResultCode SetVariable(Int32 bindingIndex, IBuffer* pBuffer) override;

        void Reset() override;

        [[nodiscard]] inline VkPipelineLayout GetNativePipelineLayout() const
        {
            return m_PipelineLayout;
        }

        [[nodiscard]] inline VkDescriptorSet GetNativeDescriptorSet() const
        {
            return m_DescriptorSet;
        }

        static ResultCode Create(IComputeDevice* pDevice, IResourceBinding** ppResourceBinding);
    };
} // namespace UN
