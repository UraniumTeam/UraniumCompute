#pragma once
#include <UnCompute/Backend/DeviceMemoryBase.h>
#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>

namespace UN
{
    class VulkanDeviceMemory final : public DeviceMemoryBase
    {
        VkDeviceMemory m_NativeMemory = VK_NULL_HANDLE;
        UInt32 m_MemoryTypeIndex      = 0;
        bool m_Mapped                 = false;

    protected:
        ResultCode InitInternal(const DescriptorType& desc) override;

    public:
        explicit VulkanDeviceMemory(IComputeDevice* pDevice);
        ~VulkanDeviceMemory() override;

        ResultCode Map(UInt64 byteOffset, UInt64 byteSize, void** ppData) override;
        void Unmap() override;
        bool IsCompatible(IDeviceObject* pObject, UInt64 sizeLimit) override;
        bool IsCompatible(IDeviceObject* pObject) override;
        void Reset() override;

        [[nodiscard]] inline VkDeviceMemory GetNativeMemory() const
        {
            return m_NativeMemory;
        }

        inline static ResultCode Create(IComputeDevice* pDevice, IDeviceMemory** ppMemory)
        {
            *ppMemory = AllocateObject<VulkanDeviceMemory>(pDevice);
            (*ppMemory)->AddRef();
            return ResultCode::Success;
        }
    };
} // namespace UN
