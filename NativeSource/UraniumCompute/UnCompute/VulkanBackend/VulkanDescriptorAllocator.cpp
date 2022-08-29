#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanDescriptorAllocator.h>

namespace UN
{
    VulkanDescriptorAllocator::VulkanDescriptorAllocator(IComputeDevice* pDevice)
        : DeviceObjectBase(pDevice)
    {
    }

    VulkanDescriptorAllocator::~VulkanDescriptorAllocator()
    {
        Reset();
    }

    void VulkanDescriptorAllocator::Reset()
    {
        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        for (auto pool : m_FreePools)
        {
            vkDestroyDescriptorPool(vkDevice, pool, nullptr);
        }

        for (auto pool : m_UsedPools)
        {
            vkDestroyDescriptorPool(vkDevice, pool, nullptr);
        }
    }

    ResultCode VulkanDescriptorAllocator::Init(const DescriptorType& desc)
    {
        DeviceObjectBase::Init("VulkanDescriptorAllocator", desc);
        return ResultCode::Success;
    }

    ResultCode VulkanDescriptorAllocator::AllocateSet(VkDescriptorSetLayout setLayout, VkDescriptorSet* pDescriptorSet)
    {
        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        if (m_CurrentPool == VK_NULL_HANDLE)
        {
            m_CurrentPool = GetPool();
            m_UsedPools.push_back(m_CurrentPool);
        }

        VkDescriptorSetAllocateInfo setAI{};
        setAI.sType              = VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO;
        setAI.pSetLayouts        = &setLayout;
        setAI.descriptorPool     = m_CurrentPool;
        setAI.descriptorSetCount = 1;

        auto result = vkAllocateDescriptorSets(vkDevice, &setAI, pDescriptorSet);
        if (Succeeded(result))
        {
            return ResultCode::Success;
        }

        if (result == VK_ERROR_FRAGMENTED_POOL || result == VK_ERROR_OUT_OF_POOL_MEMORY)
        {
            m_CurrentPool        = GetPool();
            setAI.descriptorPool = m_CurrentPool;

            result = vkAllocateDescriptorSets(vkDevice, &setAI, pDescriptorSet);
            if (Succeeded(result))
            {
                return ResultCode::Success;
            }
        }

        UN_Error(false, "Couldn't allocate Vulkan descriptor set, vkAllocateDescriptorSets returned {}", result);
        return VulkanConvert(result);
    }

    VkDescriptorPool VulkanDescriptorAllocator::CreatePool(UInt32 count, VkDescriptorPoolCreateFlags flags)
    {
        std::array<VkDescriptorPoolSize, DescriptorTypeMaxValue> sizes{};
        UInt32 sizeCount = 0;
        for (Int32 i = 0; i < DescriptorTypeMaxValue; ++i)
        {
            auto descriptorCount = static_cast<UInt32>(static_cast<float>(count) * m_Desc.Sizes[i]);
            if (descriptorCount > 0)
            {
                auto& size           = sizes[sizeCount++];
                size.type            = static_cast<VkDescriptorType>(i);
                size.descriptorCount = descriptorCount;
            }
        }

        VkDescriptorPoolCreateInfo poolCI{};
        poolCI.sType         = VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO;
        poolCI.flags         = flags;
        poolCI.maxSets       = count;
        poolCI.poolSizeCount = sizeCount;
        poolCI.pPoolSizes    = sizes.data();

        VkDescriptorPool descriptorPool;
        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        auto result   = vkCreateDescriptorPool(vkDevice, &poolCI, nullptr, &descriptorPool);
        UN_Assert(Succeeded(result), "Couldn't create a Vulkan descriptor pool, vkCreateDescriptorPool returned {}", result);

        return descriptorPool;
    }

    VkDescriptorPool VulkanDescriptorAllocator::GetPool()
    {
        if (m_FreePools.empty())
        {
            return CreatePool(GetPoolSize(), m_Desc.PoolFlags);
        }

        auto pool = m_FreePools.back();
        m_FreePools.pop_back();
        return pool;
    }

    ResultCode VulkanDescriptorAllocator::Create(IComputeDevice* pDevice, VulkanDescriptorAllocator** ppDescriptorAllocator)
    {
        *ppDescriptorAllocator = AllocateObject<VulkanDescriptorAllocator>(pDevice);
        (*ppDescriptorAllocator)->AddRef();
        return ResultCode::Success;
    }

    void VulkanDescriptorAllocator::ResetPools()
    {
        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        for (auto pool : m_UsedPools)
        {
            vkResetDescriptorPool(vkDevice, pool, VK_FLAGS_NONE);
        }

        m_FreePools = m_UsedPools;
        m_UsedPools.clear();
        m_CurrentPool = VK_NULL_HANDLE;
    }
} // namespace UN
