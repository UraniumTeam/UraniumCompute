#include <UnCompute/Acceleration/DeviceFactory.h>

namespace UN
{
    DeviceFactory::DeviceFactory(BackendKind backendKind)
        : m_BackendKind(backendKind)
    {
    }
} // namespace UN
