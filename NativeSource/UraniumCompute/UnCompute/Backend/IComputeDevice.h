#pragma once
#include <UnCompute/Backend/BaseTypes.h>
#include <UnCompute/Backend/IDeviceObject.h>

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

    class IFence;
    class IBuffer;
    class IDeviceMemory;
    class ICommandList;
    class IResourceBinding;

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

        virtual ResultCode CreateBuffer(IBuffer** ppBuffer) = 0;

        virtual ResultCode CreateMemory(IDeviceMemory** ppMemory) = 0;

        virtual ResultCode CreateFence(IFence** ppFence) = 0;

        virtual ResultCode CreateCommandList(ICommandList** ppCommandList) = 0;

        virtual ResultCode CreateResourceBinding(IResourceBinding** ppResourceBinding) = 0;
    };
} // namespace UN
