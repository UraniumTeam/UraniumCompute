#pragma once
#include <UnCompute/Acceleration/IDeviceFactory.h>

namespace UN
{
    //! \brief Implementation of IDeviceFactory.
    class DeviceFactory final : public Object<IDeviceFactory>
    {
        BackendKind m_BackendKind;

    public:
        ResultCode Init(BackendKind backendKind) override;

        [[nodiscard]] BackendKind GetBackendKind() const override;
        ResultCode CreateDevice(const ComputeDeviceDesc& desc, IComputeDevice** ppDevice) override;
    };
} // namespace UN
