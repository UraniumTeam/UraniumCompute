#pragma once
#include <UnCompute/Base/Flags.h>

namespace UN
{
    //! \brief Flags that store the kinds of operations that can be executed using a GPU hardware queue.
    enum class HardwareQueueKindFlags
    {
        None = 0, //!< Invalid or unspecified value.

        GraphicsBit = UN_BIT(0), //!< Queue that supports graphics operations.
        ComputeBit  = UN_BIT(1), //!< Queue that supports compute operations.
        TransferBit = UN_BIT(2), //!< Queue that supports copy operations.

        //! \brief Queue for graphics + compute + copy operations.
        Graphics = GraphicsBit | ComputeBit | TransferBit,
        //! \brief Queue for compute + copy operations.
        Compute = ComputeBit | TransferBit,
        //! \brief Queue for copy operations.
        Transfer = TransferBit
    };

    UN_ENUM_OPERATORS(HardwareQueueKindFlags);
} // namespace UN
