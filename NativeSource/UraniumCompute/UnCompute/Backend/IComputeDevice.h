#pragma once
#include <UnCompute/Backend/IDeviceObject.h>
#include <UnCompute/Base/Flags.h>

namespace UN
{
    //! \brief Compute device descriptor.
    struct ComputeDeviceDesc
    {
        UInt32 AdapterId; //!< ID of the adapter to create the device on.

        inline ComputeDeviceDesc()
            : AdapterId(0)
        {
        }

        inline explicit ComputeDeviceDesc(UInt32 adapterId)
            : AdapterId(adapterId)
        {
        }
    };

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

    //! \brief Interface for all backend-specific compute devices.
    //!
    //! Compute device is an object that allows users to create data buffers, synchronization primitives and other objects
    //! using the target backend. It allows to run compute kernels using ICommandList interface.
    //!
    //! Compute devices are a part of UraniumCompute low-level API and should not be used directly together with job graphs
    //! and other higher-level objects.
    class IComputeDevice : public IObject
    {
    public:
        ~IComputeDevice() override = default;

        //! \brief Initializes the compute device with the provided descriptor.
        //!
        //! \param desc - A compute device descriptor.
        virtual ResultCode Init(const ComputeDeviceDesc& desc) = 0;

        //! \brief Reset the object to uninitialized state.
        virtual void Reset() = 0;
    };
} // namespace UN
