#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanBuffer.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanDescriptorAllocator.h>
#include <UnCompute/VulkanBackend/VulkanResourceBinding.h>
#include <UnCompute/VulkanBackend/VulkanDeviceMemory.h>

namespace UN
{
    inline VkDescriptorType GetDescriptorType(KernelResourceKind kind)
    {
        switch (kind)
        {
        case KernelResourceKind::Buffer:
            return VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER;
        case KernelResourceKind::ConstantBuffer:
            return VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER;
        case KernelResourceKind::RWBuffer:
            return VK_DESCRIPTOR_TYPE_STORAGE_BUFFER;
        case KernelResourceKind::SampledTexture:
            return VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE;
        case KernelResourceKind::RWTexture:
            return VK_DESCRIPTOR_TYPE_STORAGE_IMAGE;
        case KernelResourceKind::Sampler:
            return VK_DESCRIPTOR_TYPE_SAMPLER;
        default:
            UN_Error(false, "Unknown KernelResourceKind::<{}>", static_cast<Int32>(kind));
            return VK_DESCRIPTOR_TYPE_MAX_ENUM;
        }
    }

    VulkanResourceBinding::VulkanResourceBinding(IComputeDevice* pDevice)
        : ResourceBindingBase(pDevice)
    {
    }

    void VulkanResourceBinding::Reset()
    {
        if (m_DescriptorSet == VK_NULL_HANDLE)
        {
            return;
        }

        m_DescriptorSet = VK_NULL_HANDLE;
        auto vkDevice   = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        vkDestroyDescriptorSetLayout(vkDevice, m_SetLayout, nullptr);
    }

    ResultCode VulkanResourceBinding::InitInternal(const DescriptorType& desc)
    {
        std::vector<VkDescriptorSetLayoutBinding> bindings;
        for (UInt32 i = 0; i < desc.Layout.Length(); ++i)
        {
            auto& d       = desc.Layout[i];
            auto& binding = bindings.emplace_back();

            binding.binding         = d.BindingIndex;
            binding.descriptorCount = 1;
            binding.descriptorType  = GetDescriptorType(d.Kind);
            binding.stageFlags      = VK_SHADER_STAGE_COMPUTE_BIT;
        }

        VkDescriptorSetLayoutCreateInfo layoutCI{};
        layoutCI.sType        = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO;
        layoutCI.bindingCount = static_cast<UInt32>(bindings.size());
        layoutCI.pBindings    = bindings.data();

        auto device   = m_pDevice.As<VulkanComputeDevice>();
        auto vkDevice = device->GetNativeDevice();

        if (auto result = vkCreateDescriptorSetLayout(vkDevice, &layoutCI, VK_NULL_HANDLE, &m_SetLayout); Failed(result))
        {
            UN_Error(false, "Couldn't create Vulkan descriptor set layout, vkCreateDescriptorSetLayout returned {}", result);
            return VulkanConvert(result);
        }

        auto allocator = device->GetDescriptorAllocator();
        if (auto result = allocator->AllocateSet(m_SetLayout, &m_DescriptorSet); Failed(result))
        {
            return result;
        }

        VkPipelineLayoutCreateInfo pipelineLayoutCI{};
        pipelineLayoutCI.sType          = VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO;
        pipelineLayoutCI.pSetLayouts    = &m_SetLayout;
        pipelineLayoutCI.setLayoutCount = 1;

        if (auto result = vkCreatePipelineLayout(vkDevice, &pipelineLayoutCI, nullptr, &m_PipelineLayout); Failed(result))
        {
            UN_Error(false, "Couldn't create Vulkan compute pipeline layout, vkCreatePipelineLayout returned {}", result);
            return VulkanConvert(result);
        }

        return ResultCode::Success;
    }

    ResultCode VulkanResourceBinding::Create(IComputeDevice* pDevice, IResourceBinding** ppResourceBinding)
    {
        *ppResourceBinding = AllocateObject<VulkanResourceBinding>(pDevice);
        (*ppResourceBinding)->AddRef();
        return ResultCode::Success;
    }

    VulkanResourceBinding::~VulkanResourceBinding()
    {
        Reset();
    }

    ResultCode VulkanResourceBinding::SetVariable(Int32 bindingIndex, IBuffer* pBuffer)
    {
        auto* pBinding = std::find_if(m_Desc.Layout.begin(), m_Desc.Layout.end(), [bindingIndex](const KernelResourceDesc& desc) {
            return desc.BindingIndex == bindingIndex;
        });

        if (pBinding == m_Desc.Layout.end())
        {
            UN_Error(false, "No variable at binding index {} found in RB \"{}\"", bindingIndex, GetDebugName());
            return ResultCode::InvalidArguments;
        }

        switch (pBinding->Kind)
        {
        case KernelResourceKind::Buffer:
        case KernelResourceKind::ConstantBuffer:
        case KernelResourceKind::RWBuffer:
            break;
        case KernelResourceKind::SampledTexture:
        case KernelResourceKind::RWTexture:
        case KernelResourceKind::Sampler:
            UN_Error(false, "Variable at binding index {} from RB \"{}\" was not a buffer", bindingIndex, GetDebugName());
            return ResultCode::InvalidArguments;
        }

        auto* pVkBuffer = un_verify_cast<VulkanBuffer*>(pBuffer);

        VkDescriptorBufferInfo bufferInfo{};
        bufferInfo.buffer = pVkBuffer->GetNativeBuffer();
        bufferInfo.offset = 0;
        bufferInfo.range  = VK_WHOLE_SIZE;

        VkWriteDescriptorSet writeDescriptorSet{};
        writeDescriptorSet.sType           = VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET;
        writeDescriptorSet.dstSet          = m_DescriptorSet;
        writeDescriptorSet.descriptorType  = GetDescriptorType(pBinding->Kind);
        writeDescriptorSet.dstBinding      = bindingIndex;
        writeDescriptorSet.pBufferInfo     = &bufferInfo;
        writeDescriptorSet.descriptorCount = 1;

        auto* vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        vkUpdateDescriptorSets(vkDevice, 1, &writeDescriptorSet, 0, nullptr);
        return ResultCode::Success;
    }
} // namespace UN
