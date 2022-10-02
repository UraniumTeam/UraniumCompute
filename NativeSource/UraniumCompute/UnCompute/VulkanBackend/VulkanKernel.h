#pragma once
#include <UnCompute/Backend/KernelBase.h>
#include <UnCompute/Containers/HeapArray.h>
#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>

namespace UN
{
    class VulkanResourceBinding;

    class VulkanKernel final : public KernelBase
    {
        VkPipeline m_Pipeline           = VK_NULL_HANDLE;
        VkPipelineCache m_PipelineCache = VK_NULL_HANDLE;
        VkShaderModule m_ShaderModule   = VK_NULL_HANDLE;

        Ptr<VulkanResourceBinding> m_pResourceBinding;
        HeapArray<UInt32> m_ShaderBytecode;

    protected:
        ResultCode InitInternal(const DescriptorType& desc) override;

    public:
        explicit VulkanKernel(IComputeDevice* pDevice);
        ~VulkanKernel() override;

        void Reset() override;

        [[nodiscard]] inline VkPipeline GetNativePipeline() const
        {
            return m_Pipeline;
        }

        [[nodiscard]] inline VulkanResourceBinding* GetResourceBinding() const
        {
            return m_pResourceBinding.Get();
        }

        inline static ResultCode Create(IComputeDevice* pDevice, IKernel** ppKernel)
        {
            *ppKernel = AllocateObject<VulkanKernel>(pDevice);
            (*ppKernel)->AddRef();
            return ResultCode::Success;
        }
    };
} // namespace UN
