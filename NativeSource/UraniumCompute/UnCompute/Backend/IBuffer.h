#pragma once
#include <UnCompute/Backend/IDeviceObject.h>

namespace UN
{
    //! \brief Buffer usage type.
    enum class BufferUsage
    {
        Storage, //!< The buffer is used as a storage for an array of elements.
        Constant //!< The buffer is used to store kernel constants.
    };

    //! \brief Buffer descriptor.
    struct BufferDesc
    {
        const char* Name  = nullptr;              //!< Buffer debug name.
        UInt64 Size       = 0;                    //!< Buffer size in bytes.
        BufferUsage Usage = BufferUsage::Storage; //!< Buffer usage type.

        inline BufferDesc() = default;

        inline BufferDesc(const char* name, UInt64 size, BufferUsage usage = BufferUsage::Storage)
            : Name(name)
            , Size(size)
            , Usage(usage)
        {
        }
    };

    class IDeviceMemory;
    class DeviceMemorySlice;

    //! \brief An interface for backend-specific buffers that store the data on the device.
    class IBuffer : public IDeviceObject
    {
    public:
        using DescriptorType = BufferDesc;

        [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;

        //! \brief Creates and initializes a backend-specific buffer object.
        //!
        //! \param desc - Buffer descriptor.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode Init(const BufferDesc& desc) = 0;

        //! \brief Bind device memory to the buffer.
        //!
        //! Buffer doesn't allocate any device memory itself on creation or initialization.
        //! So the memory must be allocated separately and than bound to the buffer using this function.
        //!
        //! \param deviceMemory - The memory to bind.
        //!
        //! \return ResultCode::Success or an error code (if the memory was incompatible).
        virtual ResultCode BindMemory(const DeviceMemorySlice& deviceMemory) = 0;

        //! \brief Bind device memory to the buffer.
        //!
        //! Buffer doesn't allocate any device memory itself on creation or initialization.
        //! So the memory must be allocated separately and than bound to the buffer using this function.
        //!
        //! \param pDeviceMemory - The memory to bind.
        //!
        //! \return ResultCode::Success or an error code (if the memory was incompatible).
        virtual ResultCode BindMemory(IDeviceMemory* pDeviceMemory) = 0;
    };
} // namespace UN
