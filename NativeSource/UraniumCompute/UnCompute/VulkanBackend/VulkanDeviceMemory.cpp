#include <UnCompute/VulkanBackend/VulkanBuffer.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanDeviceMemory.h>

namespace UN
{
    VkMemoryPropertyFlags VulkanConvert(MemoryKindFlags type)
    {
        switch (type)
        {
        case MemoryKindFlags::DeviceAccessible:
            return VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT;
        case MemoryKindFlags::HostAndDeviceAccessible:
            return VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT;
        default:
            UN_Error(false, "Invalid memory type");
            return 0;
        }
    }

    VulkanDeviceMemory::VulkanDeviceMemory(IComputeDevice* pDevice)
        : DeviceMemoryBase(pDevice)
    {
    }

    VkMappedMemoryRange VulkanDeviceMemory::GetMappedMemoryRange()
    {
        VkMappedMemoryRange range{};
        range.sType  = VK_STRUCTURE_TYPE_MAPPED_MEMORY_RANGE;
        range.memory = m_NativeMemory;
        range.offset = m_MapByteOffset;
        range.size   = m_MapByteSize;
        return range;
    }

    ResultCode VulkanDeviceMemory::Map(UInt64 byteOffset, UInt64 byteSize, void** ppData)
    {
        if (m_Mapped)
        {
            Unmap();
        }

        m_MapByteOffset = byteOffset;
        m_MapByteSize   = byteSize;

        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();

        auto vkResult = vkMapMemory(vkDevice, m_NativeMemory, byteOffset, byteSize, VK_FLAGS_NONE, ppData);
        m_Mapped      = Succeeded(vkResult);
        UN_Error(m_Mapped, "Couldn't map Vulkan memory, vkMapMemory returned {}", vkResult);

        auto range = GetMappedMemoryRange();
        vkInvalidateMappedMemoryRanges(vkDevice, 1, &range);
        return VulkanConvert(vkResult);
    }

    void VulkanDeviceMemory::Unmap()
    {
        if (!m_Mapped)
        {
            return;
        }

        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        auto range    = GetMappedMemoryRange();
        vkFlushMappedMemoryRanges(vkDevice, 1, &range);

        vkUnmapMemory(vkDevice, m_NativeMemory);
        m_Mapped = false;
    }

    bool VulkanDeviceMemory::IsCompatible(IDeviceObject* pObject, UInt64 sizeLimit)
    {
        auto requirements = un_verify_cast<VulkanBuffer*>(pObject)->GetMemoryRequirements();
        return requirements.size <= sizeLimit && (requirements.memoryTypeBits & (1u << m_MemoryTypeIndex)) != 0;
    }

    bool VulkanDeviceMemory::IsCompatible(IDeviceObject* pObject)
    {
        return IsCompatible(pObject, WholeSize);
    }

    void VulkanDeviceMemory::Reset()
    {
        if (m_NativeMemory != VK_NULL_HANDLE)
        {
            vkFreeMemory(m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice(), m_NativeMemory, VK_NULL_HANDLE);
            m_NativeMemory = VK_NULL_HANDLE;
        }
    }

    ResultCode VulkanDeviceMemory::InitInternal(const IDeviceMemory::DescriptorType& desc)
    {
        auto properties = VulkanConvert(desc.Flags);
        auto* vkDevice  = m_pDevice.As<VulkanComputeDevice>();

        constexpr UInt32 InvalidTypeBits = std::numeric_limits<UInt32>::max();

        auto typeBits = InvalidTypeBits;
        if (desc.Objects.Empty())
        {
            UN_Error(false, "DeviceMemoryDesc::Objects must have at least one object");
            return ResultCode::InvalidArguments;
        }

        UInt64 objectSize = 0;
        UInt64 alignment  = 1;
        for (const auto* object : desc.Objects)
        {
            auto* buffer = un_verify_cast<const VulkanBuffer*>(object);
            typeBits &= buffer->GetMemoryRequirements().memoryTypeBits;
            objectSize += buffer->GetMemoryRequirements().size;
            alignment = std::max(alignment, buffer->GetMemoryRequirements().alignment);
        }

        if (typeBits == 0)
        {
            UN_Error(false, "Some resources in DeviceMemoryDesc::Objects have incompatible requirements");
            return ResultCode::InvalidArguments;
        }

        UN_Warning(desc.Size == 0 || objectSize <= desc.Size,
                   "DeviceMemoryDesc::Size was not enough to allocate all of DeviceMemoryDesc::Objects, use zero for auto size");
        m_Desc.Size = std::max(desc.Size, objectSize);

        VkMemoryAllocateInfo info{};
        info.sType          = VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO;
        info.allocationSize = m_Desc.Size;
        UN_VerifyResult(vkDevice->FindMemoryType(typeBits, properties, m_MemoryTypeIndex), "Couldn't find device memory type");
        info.memoryTypeIndex = m_MemoryTypeIndex;

        if (auto vkResult = vkAllocateMemory(vkDevice->GetNativeDevice(), &info, nullptr, &m_NativeMemory); Failed(vkResult))
        {
            UN_Error(false, "Couldn't allocate Vulkan device memory, vkAllocateMemory returned {}", vkResult);
            return VulkanConvert(vkResult);
        }

        return ResultCode::Success;
    }

    VulkanDeviceMemory::~VulkanDeviceMemory()
    {
        Reset();
    }
} // namespace UN
