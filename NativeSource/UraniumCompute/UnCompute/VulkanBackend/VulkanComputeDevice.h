#pragma once
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Memory/Ptr.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>

namespace UN
{
    struct VulkanQueueFamily
    {
        UInt32 FamilyIndex;
        UInt32 QueueCount;
        HardwareQueueKindFlags KindFlags;
        VkCommandPool CmdPool = VK_NULL_HANDLE;

        inline VulkanQueueFamily(UInt32 familyIndex, UInt32 queueCount, HardwareQueueKindFlags kindFlags)
            : FamilyIndex(familyIndex)
            , QueueCount(queueCount)
            , KindFlags(kindFlags)
        {
        }
    };

    class VulkanDeviceFactory;

    class VulkanComputeDevice : public Object<IComputeDevice>
    {
        Ptr<VulkanDeviceFactory> m_pFactory;

        std::vector<VulkanQueueFamily> m_QueueFamilies;

        VkDevice m_NativeDevice          = VK_NULL_HANDLE;
        VkPhysicalDevice m_NativeAdapter = VK_NULL_HANDLE;

        VkMemoryRequirements m_BufferMemoryRequirements = {};

        void ResetInternal();
        void FindQueueFamilies();
        ResultCode FindMemoryType(UInt32 typeBits, VkMemoryPropertyFlags properties, UInt32& memoryType);

    public:
        using DescriptorType = ComputeDeviceDesc;

        explicit VulkanComputeDevice(VulkanDeviceFactory* pInstance);
        ~VulkanComputeDevice() override;

        ResultCode Init(const ComputeDeviceDesc& desc) override;
        void Reset() override;

        static ResultCode Create(VulkanDeviceFactory* pInstance, VulkanComputeDevice** ppDevice);
    };
} // namespace UN
