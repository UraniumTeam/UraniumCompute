#include <UnCompute/VulkanBackend/VulkanBuffer.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanDeviceMemory.h>

namespace UN
{
    VulkanBuffer::VulkanBuffer(IComputeDevice* pDevice)
        : BufferBase(pDevice)
    {
    }

    ResultCode VulkanBuffer::BindMemory(const DeviceMemorySlice& deviceMemory)
    {
        if (!deviceMemory.IsCompatible(this))
        {
            UN_Error(false, "Incompatible memory");
            return ResultCode::Fail;
        }

        m_Memory      = deviceMemory;
        m_MemoryOwner = un_verify_cast<VulkanDeviceMemory*>(m_Memory.GetDeviceMemory());
        auto vkMemory = m_MemoryOwner->GetNativeMemory();
        if (auto vkResult = vkBindBufferMemory(
                m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice(), m_NativeBuffer, vkMemory, deviceMemory.GetByteOffset());
            Failed(vkResult))
        {
            UN_Error(false, "Couldn't bind Vulkan memory to buffer, vkBindBufferMemory returned {}", vkResult);
            return VulkanConvert(vkResult);
        }

        return ResultCode::Success;
    }

    void VulkanBuffer::Reset()
    {
        if (m_NativeBuffer != VK_NULL_HANDLE)
        {
            vkDestroyBuffer(m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice(), m_NativeBuffer, nullptr);
            m_NativeBuffer = VK_NULL_HANDLE;
        }
    }

    ResultCode VulkanBuffer::InitInternal(const BufferDesc& desc)
    {
        VkBufferCreateInfo bufferCI{};
        bufferCI.sType = VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO;
        bufferCI.size  = desc.Size;
        bufferCI.usage = VK_BUFFER_USAGE_TRANSFER_DST_BIT | VK_BUFFER_USAGE_TRANSFER_SRC_BIT;

        if (desc.Usage == BufferUsage::Storage)
        {
            bufferCI.usage |= VK_BUFFER_USAGE_STORAGE_BUFFER_BIT | VK_BUFFER_USAGE_STORAGE_TEXEL_BUFFER_BIT;
        }
        else if (desc.Usage == BufferUsage::Constant)
        {
            bufferCI.usage |= VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT;
        }
        else
        {
            UN_Error(false, "Unknown buffer usage type <{}>", static_cast<Int32>(desc.Usage));
        }

        auto vkDevice = m_pDevice.As<VulkanComputeDevice>()->GetNativeDevice();
        if (auto vkResult = vkCreateBuffer(vkDevice, &bufferCI, VK_NULL_HANDLE, &m_NativeBuffer); Failed(vkResult))
        {
            UN_Error(false, "Couldn't create Vulkan buffer, vkCreateBuffer returned {}", vkResult);
            return VulkanConvert(vkResult);
        }

        vkGetBufferMemoryRequirements(vkDevice, m_NativeBuffer, &m_MemoryRequirements);
        return ResultCode::Success;
    }

    VulkanBuffer::~VulkanBuffer()
    {
        Reset();
    }

    ResultCode VulkanBuffer::BindMemory(IDeviceMemory* pDeviceMemory)
    {
        return BindMemory(DeviceMemorySlice(pDeviceMemory));
    }
} // namespace UN
