#pragma once
#include <UnCompute/Backend/BufferBase.h>
#include <UnCompute/Backend/IDeviceMemory.h>
#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>

namespace UN
{
    class VulkanDeviceMemory;

    class VulkanBuffer final : public BufferBase
    {
        VkBuffer m_NativeBuffer                   = VK_NULL_HANDLE;
        VkMemoryRequirements m_MemoryRequirements = {};
        Ptr<VulkanDeviceMemory> m_MemoryOwner     = {}; // !!! must be here to not free the memory before ~DeviceMemorySlice()
        DeviceMemorySlice m_Memory                = {};

    protected:
        ResultCode InitInternal(const BufferDesc& desc) override;

    public:
        explicit VulkanBuffer(IComputeDevice* pDevice);
        ~VulkanBuffer() override;

        ResultCode BindMemory(const DeviceMemorySlice& deviceMemory) override;
        void Reset() override;

        [[nodiscard]] inline VkBuffer GetNativeBuffer() const
        {
            return m_NativeBuffer;
        }

        [[nodiscard]] inline const VkMemoryRequirements& GetMemoryRequirements() const
        {
            return m_MemoryRequirements;
        }

        inline static ResultCode Create(IComputeDevice* pDevice, IBuffer** ppBuffer)
        {
            *ppBuffer = AllocateObject<VulkanBuffer>(pDevice);
            (*ppBuffer)->AddRef();
            return ResultCode::Success;
        }
    };
} // namespace UN
