#include <UnCompute/Acceleration/DeviceFactory.h>
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>

namespace UN
{
    ResultCode DeviceFactory::Init(BackendKind backendKind)
    {
        m_BackendKind = backendKind;
        auto result   = VulkanInstance::Create(&m_pVulkanInstance);
        if (UN_Succeeded(result))
        {
            return m_pVulkanInstance->Init("UraniumCompute");
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

    ResultCode DeviceFactory::CreateDevice(IComputeDevice** ppDevice)
    {
        *ppDevice = nullptr;

        switch (m_BackendKind)
        {
        case BackendKind::Cpu:
            break;
        case BackendKind::Vulkan:
            {
                VulkanComputeDevice* pResult;
                auto resultCode = VulkanComputeDevice::Create(m_pVulkanInstance.Get(), &pResult);
                *ppDevice       = pResult;
                return resultCode;
            }
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
