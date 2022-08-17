#pragma once
#include <UnCompute/Memory/Object.h>

namespace UN
{
    //! \brief Base interface for all compute backend objects.
    //!
    //! All object related to a compute backend must implement deferred initialization.
    //! For example:
    //! \code{.cpp}
    //! class IBuffer : public IDeviceObject
    //! {
    //! public:
    //!     virtual void Init(const BufferDesc& desc) = 0;
    //! };
    //!
    //! class CpuBuffer : public Object<IBuffer>
    //! {
    //! public:
    //!     static Ptr<CpuBuffer> Create() { ... }             // doesn't create any device objects
    //!     void Init(const BufferDesc& desc) override { ... } // creates device objects, allocates memory, etc.
    //!     void Reset() override { ... }                      // releases device objects and memory
    //! };
    //! \endcode
    class IDeviceObject : public IObject
    {
    public:
        ~IDeviceObject() override = default;

        //! \brief Reset the object to uninitialized state.
        virtual void Reset() = 0;
    };
} // namespace UN
