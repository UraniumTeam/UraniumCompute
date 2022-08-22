#pragma once
#include <UnCompute/Memory/Object.h>

namespace UN
{
    class IComputeDevice;

    //! \brief Base interface for all compute backend objects.
    //!
    //! All objects related to a compute backend must implement deferred initialization.
    //! For example:
    //! \code{.cpp}
    //!     class IBuffer : public IDeviceObject
    //!     {
    //!     public:
    //!         using DescriptorType = BufferDesc;
    //!         virtual void Init(const DescriptorType& desc) = 0;
    //!         virtual const DescriptorType& GetDesc()       = 0;
    //!     };
    //!
    //!     class Buffer : public DeviceObjectBase<IBuffer>
    //!     {
    //!     public:
    //!         static ResultCode Create(IBuffer** ppBuffer) { ... } // doesn't create any device objects
    //!         void Init(const BufferDesc& desc) override   { ... } // creates device objects, allocates memory, etc.
    //!         void Reset() override                        { ... } // releases device objects and memory
    //!     };
    //! \endcode
    class IDeviceObject : public IObject
    {
    public:
        ~IDeviceObject() override = default;

        //! \brief Reset the object to uninitialized state.
        virtual void Reset() = 0;

        //! \brief Get the compute device this object was created on.
        //!
        //! \note This function doesn't increment reference counter of the compute device.
        [[nodiscard]] virtual IComputeDevice* GetDevice() const = 0;
    };
} // namespace UN
