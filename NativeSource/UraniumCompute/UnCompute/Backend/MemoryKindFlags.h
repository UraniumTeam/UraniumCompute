#pragma once
#include <UnCompute/Base/Flags.h>

namespace UN
{
    //! \brief Device memory kind flags.
    enum class MemoryKindFlags
    {
        None             = 0,         //!< Invalid or unspecified value.
        HostAccessible   = UN_BIT(0), //!< Host (CPU) accessible memory.
        DeviceAccessible = UN_BIT(1), //!< Device (GPU for accelerated backends) accessible memory.

        HostAndDeviceAccessible = HostAccessible | DeviceAccessible //!< Memory accessible for both the device and the host.
    };
}
