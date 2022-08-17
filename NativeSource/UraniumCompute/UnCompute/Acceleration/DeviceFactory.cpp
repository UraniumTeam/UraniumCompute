#include <UnCompute/Acceleration/DeviceFactory.h>
#include <UnCompute/Backend/IComputeDevice.h>

namespace UN
{
    ResultCode DeviceFactory::Init(BackendKind backendKind)
    {
        m_BackendKind = backendKind;
        return ResultCode::Success;
    }

    ResultCode DeviceFactory::CreateDevice(const ComputeDeviceDesc& desc, IComputeDevice** result)
    {
        *result = nullptr;

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
