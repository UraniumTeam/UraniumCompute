#pragma once
#include <UnCompute/Base/Base.h>

namespace UN
{
    //! \brief Kind of adapter.
    enum class AdapterKind
    {
        None,
        Integrated,
        Discrete,
        Virtual,
        Cpu
    };

    //! \brief Information about an adapter.
    struct AdapterInfo
    {
        UInt32 Id;        //!< Adapter ID, used to create a compute device on it.
        AdapterKind Kind; //!< Kind of adapter (integrated, discrete, etc.)
        char Name[256];   //!< Name of adapter.
    };
} // namespace UN
