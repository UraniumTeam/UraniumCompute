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

    public:
        using DescriptorType = ComputeDeviceDesc;

        explicit VulkanComputeDevice(VulkanDeviceFactory* pInstance);
        ~VulkanComputeDevice() override;

        ResultCode Init(const DescriptorType& desc) override;
        void Reset() override;

        inline VkCommandPool GetCommandPool(UInt32 queueFamilyIndex)
        {
            for (auto& queue : m_QueueFamilies)
            {
                if (queue.FamilyIndex == queueFamilyIndex)
                {
                    return queue.CmdPool;
                }
            }

            UN_Verify(false, "Couldn't find command pool");
            return VK_NULL_HANDLE;
        }

        inline UInt32 GetQueueFamilyIndex(HardwareQueueKindFlags flags)
        {
            for (auto& queue : m_QueueFamilies)
            {
                if (AllFlagsActive(queue.KindFlags, flags))
                {
                    return queue.FamilyIndex;
                }
            }

            UN_Verify(false, "Couldn't find queue family");
            return std::numeric_limits<UInt32>::max();
        }

        inline VkQueue GetDeviceQueue(UInt32 queueFamilyIndex, UInt32 queueIndex)
        {
            VkQueue queue;
            vkGetDeviceQueue(m_NativeDevice, queueFamilyIndex, queueIndex, &queue);
            return queue;
        }

        inline VkQueue GetDeviceQueue(HardwareQueueKindFlags flags)
        {
            VkQueue queue;
            vkGetDeviceQueue(m_NativeDevice, GetQueueFamilyIndex(flags), 0, &queue);
            return queue;
        }

        ResultCode FindMemoryType(UInt32 typeBits, VkMemoryPropertyFlags properties, UInt32& memoryType);

        [[nodiscard]] inline VkDevice GetNativeDevice() const
        {
            return m_NativeDevice;
        }

        ResultCode CreateBuffer(IBuffer** ppBuffer) override;
        ResultCode CreateMemory(IDeviceMemory** ppMemory) override;
        ResultCode CreateFence(IFence** ppFence) override;
        ResultCode CreateCommandList(ICommandList** ppCommandList) override;

        static ResultCode Create(VulkanDeviceFactory* pInstance, VulkanComputeDevice** ppDevice);
    };
} // namespace UN
