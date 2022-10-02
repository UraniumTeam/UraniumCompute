#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanBuffer.h>
#include <UnCompute/VulkanBackend/VulkanCommandList.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanDescriptorAllocator.h>
#include <UnCompute/VulkanBackend/VulkanDeviceFactory.h>
#include <UnCompute/VulkanBackend/VulkanDeviceMemory.h>
#include <UnCompute/VulkanBackend/VulkanFence.h>
#include <UnCompute/VulkanBackend/VulkanKernel.h>
#include <UnCompute/VulkanBackend/VulkanResourceBinding.h>
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
            HardwareQueueKindFlags result{};
            if (AnyFlagsActive(flags, static_cast<VkQueueFlags>(VK_QUEUE_GRAPHICS_BIT)))
            {
                result |= HardwareQueueKindFlags::GraphicsBit;
            }
            if (AnyFlagsActive(flags, static_cast<VkQueueFlags>(VK_QUEUE_COMPUTE_BIT)))
            {
                result |= HardwareQueueKindFlags::ComputeBit;
            }
            if (AnyFlagsActive(flags, static_cast<VkQueueFlags>(VK_QUEUE_TRANSFER_BIT)))
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
        m_NativeAdapter                         = m_pFactory->GetVulkanAdapters()[desc.AdapterId];
        [[maybe_unused]] auto adapterProperties = m_pFactory->GetVulkanAdapterProperties()[desc.AdapterId];

        FindQueueFamilies();

        UInt32 availableExtCount;
        vkEnumerateDeviceExtensionProperties(m_NativeAdapter, nullptr, &availableExtCount, nullptr);

        std::vector<VkExtensionProperties> availableExt;
        availableExt.resize(availableExtCount);
        vkEnumerateDeviceExtensionProperties(m_NativeAdapter, nullptr, &availableExtCount, availableExt.data());

        for (auto& ext : RequiredDeviceExtensions)
        {
            bool found = std::any_of(availableExt.begin(), availableExt.end(), [ext](const VkExtensionProperties& props) {
                return std::string_view(ext) == props.extensionName;
            });

            if (!found)
            {
                UN_Error(false, "Vulkan device extension {} was not found", ext);
                return ResultCode::Fail;
            }
        }

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

        if (auto vkResult = vkCreateDevice(m_NativeAdapter, &deviceCI, nullptr, &m_NativeDevice); Failed(vkResult))
        {
            UN_Error(false, "Couldn't create a Vulkan device, vkCreateDevice returned {}", vkResult);
            return VulkanConvert(vkResult);
        }

        // TODO: this is not suitable for applications that use multiple VkDevice objects
        // Ok for now, but it is possible that someday we will use more than one device in our scheduler.
        volkLoadDevice(m_NativeDevice);

        for (auto& queue : m_QueueFamilies)
        {
            VkCommandPoolCreateInfo poolCI{};
            poolCI.sType            = VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO;
            poolCI.queueFamilyIndex = queue.FamilyIndex;
            poolCI.flags            = VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT;
            if (auto vkResult = vkCreateCommandPool(m_NativeDevice, &poolCI, nullptr, &queue.CmdPool); Failed(vkResult))
            {
                UN_Error(false,
                         "Couldn't create a command pool for queue family index {}, vkCreateCommandPool returned {}",
                         queue.FamilyIndex,
                         vkResult);
                return VulkanConvert(vkResult);
            }
        }

        UNLOG_Debug("Successfully created Vulkan device on {}", adapterProperties.deviceName);

        UN_VerifyResultFatal(VulkanDescriptorAllocator::Create(this, &m_pDescriptorAllocator),
                             "Couldn't create a descriptor allocator");

        VulkanDescriptorAllocatorDesc descriptorAllocatorDesc{};
        descriptorAllocatorDesc.Sizes[VK_DESCRIPTOR_TYPE_UNIFORM_TEXEL_BUFFER] = 1.f;
        descriptorAllocatorDesc.Sizes[VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER]       = 2.f;
        descriptorAllocatorDesc.Sizes[VK_DESCRIPTOR_TYPE_STORAGE_BUFFER]       = 2.f;
        descriptorAllocatorDesc.Sizes[VK_DESCRIPTOR_TYPE_SAMPLED_IMAGE]        = 1.f;
        descriptorAllocatorDesc.Sizes[VK_DESCRIPTOR_TYPE_STORAGE_IMAGE]        = 1.f;
        descriptorAllocatorDesc.Sizes[VK_DESCRIPTOR_TYPE_SAMPLER]              = 1.f;

        auto result = m_pDescriptorAllocator->Init(descriptorAllocatorDesc);
        if (Failed(result))
        {
            UN_Error(false, "Couldn't initialize a descriptor allocator, result was {}", result);
        }

        return result;
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

        for (auto& family : m_QueueFamilies)
        {
            vkDestroyCommandPool(m_NativeDevice, family.CmdPool, nullptr);
        }

        vkDestroyDevice(m_NativeDevice, nullptr);
    }

    ResultCode VulkanComputeDevice::CreateBuffer(IBuffer** ppBuffer)
    {
        return VulkanBuffer::Create(this, ppBuffer);
    }

    ResultCode VulkanComputeDevice::CreateMemory(IDeviceMemory** ppMemory)
    {
        return VulkanDeviceMemory::Create(this, ppMemory);
    }

    ResultCode VulkanComputeDevice::CreateFence(IFence** ppFence)
    {
        return VulkanFence::Create(this, ppFence);
    }

    ResultCode VulkanComputeDevice::CreateCommandList(ICommandList** ppCommandList)
    {
        return VulkanCommandList::Create(this, ppCommandList);
    }

    ResultCode VulkanComputeDevice::CreateResourceBinding(IResourceBinding** ppResourceBinding)
    {
        return VulkanResourceBinding::Create(this, ppResourceBinding);
    }

    ResultCode VulkanComputeDevice::CreateKernel(IKernel** ppKernel)
    {
        return VulkanKernel::Create(this, ppKernel);
    }
} // namespace UN
