#include <UnCompute/VulkanBackend/VulkanDeviceMemory.h>

namespace UN
{
    VulkanDeviceMemory::VulkanDeviceMemory(IComputeDevice* pDevice)
        : DeviceMemoryBase(pDevice)
    {
    }

    void* VulkanDeviceMemory::Map(UInt64 byteOffset, UInt64 byteSize)
    {
        UN_UNUSED(byteOffset);
        UN_UNUSED(byteSize);
        return nullptr;
    }
    void VulkanDeviceMemory::Unmap() {}
    bool VulkanDeviceMemory::IsCompatible(IDeviceObject* pObject, UInt64 sizeLimit)
    {
        UN_UNUSED(pObject);
        UN_UNUSED(sizeLimit);
        return false;
    }
    bool VulkanDeviceMemory::IsCompatible(IDeviceObject* pObject)
    {
        UN_UNUSED(pObject);
        return false;
    }
    void VulkanDeviceMemory::Reset() {}
    ResultCode VulkanDeviceMemory::InitInternal(const IDeviceMemory::DescriptorType& desc)
    {
        UN_UNUSED(desc);
        return ResultCode::Fail;
    }
} // namespace UN
