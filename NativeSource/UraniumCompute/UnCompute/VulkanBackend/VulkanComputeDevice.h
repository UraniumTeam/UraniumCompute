#pragma once
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Memory/Ptr.h>

namespace UN
{
    class VulkanInstance;

    class VulkanComputeDevice : public Object<IComputeDevice>
    {
        Ptr<VulkanInstance> m_Instance;

    public:
        explicit VulkanComputeDevice(VulkanInstance* pInstance);
        void Init(const ComputeDeviceDesc& desc) override;
        void Reset() override;
    };
} // namespace UN
