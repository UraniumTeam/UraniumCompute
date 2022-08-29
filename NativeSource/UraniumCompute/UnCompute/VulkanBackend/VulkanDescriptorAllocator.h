#pragma once
#include <UnCompute/Backend/DeviceObjectBase.h>
#include <UnCompute/Memory/Object.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>

namespace UN
{
    struct VulkanDescriptorAllocatorDesc
    {
        std::array<float, DescriptorTypeMaxValue> Sizes{};
        VkDescriptorPoolCreateFlags PoolFlags = VK_FLAGS_NONE;
    };

    class IVulkanDescriptorAllocator : public IDeviceObject
    {
    public:
        using DescriptorType = VulkanDescriptorAllocatorDesc;

        [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;

        virtual ResultCode Init(const DescriptorType& desc) = 0;
    };

    class VulkanDescriptorAllocator final : public DeviceObjectBase<IVulkanDescriptorAllocator>
    {
        VkDescriptorPool m_CurrentPool = VK_NULL_HANDLE;
        std::vector<VkDescriptorPool> m_UsedPools;
        std::vector<VkDescriptorPool> m_FreePools;

        UInt32 m_NextPoolSize = 128;

        inline UInt32 GetPoolSize()
        {
            auto result = m_NextPoolSize;
            m_NextPoolSize *= 2;
            return result;
        }

        VkDescriptorPool CreatePool(UInt32 count, VkDescriptorPoolCreateFlags flags);
        VkDescriptorPool GetPool();

    public:
        explicit VulkanDescriptorAllocator(IComputeDevice* pDevice);
        ~VulkanDescriptorAllocator() override;

        ResultCode Init(const DescriptorType& desc) override;
        void Reset() override;
        void ResetPools();

        ResultCode AllocateSet(VkDescriptorSetLayout setLayout, VkDescriptorSet* pDescriptorSet);

        static ResultCode Create(IComputeDevice* pDevice, VulkanDescriptorAllocator** ppDescriptorAllocator);
    };
} // namespace UN
