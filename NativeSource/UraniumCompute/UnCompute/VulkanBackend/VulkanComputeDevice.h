#pragma once
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Memory/Ptr.h>

namespace UN
{
    class VulkanInstance;

    class VulkanComputeDevice : public Object<IComputeDevice>
    {
        Ptr<VulkanInstance> m_pInstance;

    public:
        using DescriptorType = ComputeDeviceDesc;

        explicit VulkanComputeDevice(VulkanInstance* pInstance);
        ResultCode Init(const ComputeDeviceDesc& desc) override;
        void Reset() override;

        static ResultCode Create(VulkanInstance* pInstance, VulkanComputeDevice** ppDevice);
    };
} // namespace UN
