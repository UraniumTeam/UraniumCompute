#include <UnCompute/VulkanBackend/VulkanBuffer.h>
#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanDeviceFactory.h>
#include <UnCompute/VulkanBackend/VulkanDeviceMemory.h>
#include <algorithm>

namespace UN
{
    constexpr auto RequiredDeviceExtensions = std::array<const char*, 0>{};

    VulkanComputeDevice::VulkanComputeDevice(VulkanDeviceFactory* pInstance)
        : m_pFactory(pInstance)
    {
    }

    void VulkanComputeDevice::FindQueueFamilies()
    {
        auto hasQueueFamily = [this](HardwareQueueKindFlags flags) {
            return std::any_of(m_QueueFamilies.begin(), m_QueueFamilies.end(), [flags](const auto& data) {
                return data.KindFlags == flags;
            });
        };

        auto convertQueueFlags = [](VkQueueFlags flags) {
            VkQueueFlags allBits = VK_QUEUE_TRANSFER_BIT | VK_QUEUE_COMPUTE_BIT | VK_QUEUE_GRAPHICS_BIT;
            if (FlagsRemove(flags, allBits) > 0)
            {
                return HardwareQueueKindFlags::None;
            }

            HardwareQueueKindFlags result{};
            if (FlagsAny(flags, static_cast<VkQueueFlags>(VK_QUEUE_GRAPHICS_BIT)))
            {
                result |= HardwareQueueKindFlags::GraphicsBit;
            }
            if (FlagsAny(flags, static_cast<VkQueueFlags>(VK_QUEUE_COMPUTE_BIT)))
            {
                result |= HardwareQueueKindFlags::ComputeBit;
            }
            if (FlagsAny(flags, static_cast<VkQueueFlags>(VK_QUEUE_TRANSFER_BIT)))
            {
                result |= HardwareQueueKindFlags::TransferBit;
            }

            return result;
        };

        UInt32 familyCount;
        vkGetPhysicalDeviceQueueFamilyProperties(m_NativeAdapter, &familyCount, nullptr);
        std::vector<VkQueueFamilyProperties> families(familyCount, VkQueueFamilyProperties{});
        vkGetPhysicalDeviceQueueFamilyProperties(m_NativeAdapter, &familyCount, families.data());
        for (USize i = 0; i < families.size(); ++i)
        {
            auto flags = convertQueueFlags(families[i].queueFlags);
            if (!hasQueueFamily(flags))
            {
                m_QueueFamilies.emplace_back(static_cast<UInt32>(i), families[i].queueCount, flags);
            }
        }
    }

    ResultCode VulkanComputeDevice::FindMemoryType(UInt32 typeBits, VkMemoryPropertyFlags properties, UInt32& memoryType)
    {
        VkPhysicalDeviceMemoryProperties memProperties;
        vkGetPhysicalDeviceMemoryProperties(m_NativeAdapter, &memProperties);

        for (UInt32 i = 0; i < memProperties.memoryTypeCount; ++i)
        {
            if ((typeBits & (1 << i)) && (memProperties.memoryTypes[i].propertyFlags & properties) == properties)
            {
                memoryType = i;
                return ResultCode::Success;
            }
        }

        UN_Error(false, "Memory type with typeBits={} and properties={} not found", typeBits, properties);
        return ResultCode::Fail;
    }

    ResultCode VulkanComputeDevice::Init(const ComputeDeviceDesc& desc)
    {
        m_NativeAdapter        = m_pFactory->GetVulkanAdapters()[desc.AdapterId];
        auto adapterProperties = m_pFactory->GetVulkanAdapterProperties()[desc.AdapterId];

        FindQueueFamilies();

        UInt32 availableExtCount;
        vkEnumerateDeviceExtensionProperties(m_NativeAdapter, nullptr, &availableExtCount, nullptr);

        std::vector<VkExtensionProperties> availableExt;
        availableExt.resize(availableExtCount);
        vkEnumerateDeviceExtensionProperties(m_NativeAdapter, nullptr, &availableExtCount, availableExt.data());

        // We do not use any device extensions yet, and the compiler complains about unreachable code here,
        // so lets comment this out for now...

        // for (auto& ext : RequiredDeviceExtensions)
        // {
        //     bool found = std::any_of(availableExt.begin(), availableExt.end(), [ext](const VkExtensionProperties& props) {
        //         return std::string_view(ext) == props.extensionName;
        //     });
        //
        //     UN_Error(found, "Vulkan device extension {} was not found", ext);
        //     return ResultCode::Fail;
        // }

        constexpr Float32 queuePriority = 1.0f;
        std::vector<VkDeviceQueueCreateInfo> queuesCI{};
        queuesCI.reserve(m_QueueFamilies.size());
        for (auto& queue : m_QueueFamilies)
        {
            auto& queueCI            = queuesCI.emplace_back();
            queueCI.sType            = VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO;
            queueCI.queueFamilyIndex = queue.FamilyIndex;
            queueCI.queueCount       = 1;
            queueCI.pQueuePriorities = &queuePriority;
        }

        VkPhysicalDeviceFeatures deviceFeatures{};

        VkDeviceCreateInfo deviceCI{};
        deviceCI.sType                   = VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO;
        deviceCI.queueCreateInfoCount    = static_cast<UInt32>(queuesCI.size());
        deviceCI.pQueueCreateInfos       = queuesCI.data();
        deviceCI.pEnabledFeatures        = &deviceFeatures;
        deviceCI.enabledExtensionCount   = static_cast<UInt32>(RequiredDeviceExtensions.size());
        deviceCI.ppEnabledExtensionNames = RequiredDeviceExtensions.data();

        vkCreateDevice(m_NativeAdapter, &deviceCI, VK_NULL_HANDLE, &m_NativeDevice);

        // TODO: this is not suitable for applications that use multiple VkDevice objects
        // Ok for now, but it is possible that someday we will use more than one device in our scheduler.
        volkLoadDevice(m_NativeDevice);

        for (auto& queue : m_QueueFamilies)
        {
            VkCommandPoolCreateInfo poolCI{};
            poolCI.sType            = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
            poolCI.queueFamilyIndex = queue.FamilyIndex;
            vkCreateCommandPool(m_NativeDevice, &poolCI, VK_NULL_HANDLE, &queue.CmdPool);
        }

        UNLOG_Debug("Successfully created Vulkan device on {}", adapterProperties.deviceName);
        return ResultCode::Success;
    }

    void VulkanComputeDevice::Reset()
    {
        ResetInternal();
    }

    ResultCode VulkanComputeDevice::Create(VulkanDeviceFactory* pFactory, VulkanComputeDevice** ppDevice)
    {
        *ppDevice = AllocateObject<VulkanComputeDevice>(pFactory);
        (*ppDevice)->AddRef();
        return ResultCode::Success;
    }

    VulkanComputeDevice::~VulkanComputeDevice()
    {
        ResetInternal();
    }

    void VulkanComputeDevice::ResetInternal()
    {
        UNLOG_Debug("Destroyed Vulkan device");
        vkDestroyDevice(m_NativeDevice, nullptr);
    }

    ResultCode VulkanComputeDevice::CreateBuffer(IBuffer** ppBuffer)
    {
        VulkanBuffer::Create(this, ppBuffer);
        return ResultCode::Success;
    }

    ResultCode VulkanComputeDevice::CreateMemory(IDeviceMemory** ppMemory)
    {
        VulkanDeviceMemory::Create(this, ppMemory);
        return ResultCode::Fail;
    }
} // namespace UN
