#include <UnCompute/Acceleration/DeviceFactory.h>
#include <UnCompute/Backend/IComputeDevice.h>

namespace UN
{
    ResultCode DeviceFactory::Init(BackendKind backendKind)
    {
        m_BackendKind = backendKind;
        auto result   = VulkanInstance::Create(&m_pVulkanInstance);
        if (UN_SUCCEEDED(result))
        {
            m_pVulkanInstance->Init("App name");
        }

        return result;
    }

    std::vector<AdapterInfo> DeviceFactory::EnumerateAdapters()
    {
        switch (m_BackendKind)
        {
        case BackendKind::Cpu:
            return { AdapterInfo() };
        case BackendKind::Vulkan:
            return m_pVulkanInstance->EnumerateAdapters();
        default:
            return {};
        }
    }

    ResultCode DeviceFactory::CreateDevice(const ComputeDeviceDesc& desc, IComputeDevice** ppDevice)
    {
        *ppDevice = nullptr;

        (void)desc;
        switch (m_BackendKind)
        {
        case BackendKind::Cpu:
            break;
        case BackendKind::Vulkan:
            break;
        default:
            return ResultCode::InvalidArguments;
        }

        return ResultCode::NotImplemented;
    }

    BackendKind DeviceFactory::GetBackendKind() const
    {
        return m_BackendKind;
    }
} // namespace UN
