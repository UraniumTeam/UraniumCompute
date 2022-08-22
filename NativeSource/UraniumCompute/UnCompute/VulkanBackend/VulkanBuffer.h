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
        DeviceMemorySlice m_Memory                = {};
        Ptr<VulkanDeviceMemory> m_MemoryOwner     = {};

    protected:
        ResultCode InitInternal(const BufferDesc& desc) override;

    public:
        explicit VulkanBuffer(IComputeDevice* pDevice);

        ResultCode BindMemory(const DeviceMemorySlice& deviceMemory) override;
        void Reset() override;

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
