#pragma once

namespace UN
{
    enum class BackendKind
    {
        Cpu,
        Vulkan
    };

    class DeviceFactory final
    {
        BackendKind m_BackendKind;

    public:
        explicit DeviceFactory(BackendKind backendKind);

        inline BackendKind GetBackendKind()
        {
            return m_BackendKind;
        }
    };
} // namespace UN
