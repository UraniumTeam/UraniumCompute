#pragma once
#include <UnCompute/Backend/IDeviceObject.h>
#include <UnCompute/Base/Base.h>

namespace UN
{
    //! \brief Compute device descriptor.
    struct ComputeDeviceDesc
    {
        Int AdapterId; //!< ID of the adapter to create the device on.
    };

    //! \brief Interface for all backend-specific compute devices.
    //!
    //! Compute device is an object that allows users to create data buffers, synchronization primitives and other objects
    //! using the target backend. It allows to run compute kernels using ICommandList interface.
    //!
    //! Compute devices are a part of UraniumCompute low-level API and should not be used directly together with job graphs
    //! and other higher-level objects.
    class IComputeDevice : public IDeviceObject
    {
    public:
        ~IComputeDevice() override = default;

        //! \brief Initializes the compute device with the provided descriptor.
        //!
        //! \param desc - A compute device descriptor.
        virtual void Init(const ComputeDeviceDesc& desc) = 0;
    };
} // namespace UN
