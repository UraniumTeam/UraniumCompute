#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanKernel.h>
#include <UnCompute/VulkanBackend/VulkanResourceBinding.h>

namespace UN
{
    void VulkanKernel::Reset()
    {
        if (m_Pipeline == VK_NULL_HANDLE)
        {
            return;
        }

        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        vkDestroyPipeline(vkDevice, m_Pipeline, nullptr);
        vkDestroyPipelineCache(vkDevice, m_PipelineCache, nullptr);
        vkDestroyShaderModule(vkDevice, m_ShaderModule, nullptr);
        m_Pipeline      = VK_NULL_HANDLE;
        m_PipelineCache = VK_NULL_HANDLE;
        m_ShaderModule  = VK_NULL_HANDLE;
    }

    ResultCode VulkanKernel::InitInternal(const DescriptorType& desc)
    {
        m_pResourceBinding = un_verify_cast<VulkanResourceBinding*>(desc.pResourceBinding);

        auto device   = m_pDevice.As<VulkanComputeDevice>();
        auto vkDevice = device->GetNativeDevice();

        VkPipelineCacheCreateInfo cacheCI{};
        cacheCI.sType = VK_STRUCTURE_TYPE_PIPELINE_CACHE_CREATE_INFO;
        if (auto result = vkCreatePipelineCache(vkDevice, &cacheCI, nullptr, &m_PipelineCache); Failed(result))
        {
            UN_Error(false, "Couldn't create Vulkan pipeline cache, vkCreatePipelineCache returned {}", result);
            return VulkanConvert(result);
        }

        VkComputePipelineCreateInfo pipelineCI{};
        pipelineCI.sType  = VK_STRUCTURE_TYPE_COMPUTE_PIPELINE_CREATE_INFO;
        pipelineCI.layout = m_pResourceBinding->GetNativePipelineLayout();

        VkPipelineShaderStageCreateInfo shaderStage{};
        shaderStage.sType = VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO;
        shaderStage.stage = VK_SHADER_STAGE_COMPUTE_BIT;

        m_ShaderBytecode.Resize((m_Desc.Bytecode.Length() + 3) / 4);
        memcpy(m_ShaderBytecode.Data(), m_Desc.Bytecode.Data(), m_Desc.Bytecode.Length());

        VkShaderModuleCreateInfo moduleCI{};
        moduleCI.sType    = VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO;
        moduleCI.codeSize = m_Desc.Bytecode.Length();
        moduleCI.pCode    = m_ShaderBytecode.Data();

        if (auto result = vkCreateShaderModule(vkDevice, &moduleCI, nullptr, &m_ShaderModule); Failed(result))
        {
            UN_Error(false, "Couldn't create Vulkan shader module, vkCreateShaderModule returned {}", result);
            return VulkanConvert(result);
        }

        shaderStage.module = m_ShaderModule;
        shaderStage.pName  = "main";

        pipelineCI.stage = shaderStage;

        if (auto result = vkCreateComputePipelines(vkDevice, m_PipelineCache, 1, &pipelineCI, nullptr, &m_Pipeline);
            Failed(result))
        {
            UN_Error(false, "Couldn't create Vulkan compute pipeline, vkCreateComputePipelines returned {}", result);
            return VulkanConvert(result);
        }

        return ResultCode::Success;
    }

    VulkanKernel::VulkanKernel(IComputeDevice* pDevice)
        : KernelBase(pDevice)
    {
    }

    VulkanKernel::~VulkanKernel()
    {
        Reset();
    }
} // namespace UN
