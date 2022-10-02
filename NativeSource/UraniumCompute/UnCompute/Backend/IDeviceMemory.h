#pragma once
#include <UnCompute/Backend/IBuffer.h>
#include <UnCompute/Base/Flags.h>
#include <UnCompute/Containers/ArraySlice.h>
#include <limits>

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

    //! \brief Device memory descriptor.
    struct DeviceMemoryDesc
    {
        const char* Name = nullptr;               //!< Device memory debug name.
        UInt64 Size      = 0;                     //!< Memory size in bytes.
        ArraySlice<IDeviceObject*> Objects; //!< Resource objects that the memory must be compatible with.
        MemoryKindFlags Flags;                    //!< Memory kind flags.

        inline DeviceMemoryDesc() = default;

        inline DeviceMemoryDesc(const char* name, MemoryKindFlags flags, UInt64 size, ArraySlice<IDeviceObject*> objects)
            : Name(name)
            , Size(size)
            , Objects(objects)
            , Flags(flags)
        {
        }
    };

    //! \brief This class holds a handle to backend-specific memory.
    class IDeviceMemory : public IDeviceObject
    {
    public:
        using DescriptorType = DeviceMemoryDesc;

        //! \brief Special constant that represents the whole memory size.
        inline static constexpr UInt64 WholeSize = std::numeric_limits<UInt64>::max();

        [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;

        //! \brief Allocate memory with specified descriptor.
        //!
        //! \param desc - Device memory descriptor.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode Init(const DescriptorType& desc) = 0;

        //! \brief Map the device memory to access it from the host.
        //!
        //! \param byteOffset - Byte offset of the memory to map.
        //! \param byteSize   - Size of the part of the memory to map.
        //! \param ppData     - A pointer to the memory where the pointer to the mapped memory will be written.
        //!
        //! \note This function is only valid to use if the memory was allocated with MemoryKindFlags::HostAccessible flag.
        //!
        //! \return ResultCode::Success or an error node.
        virtual ResultCode Map(UInt64 byteOffset, UInt64 byteSize, void** ppData) = 0;

        //! \brief Unmap the mapped memory.
        //!
        //! \note This function does nothing if the memory was not mapped by calling Map().
        virtual void Unmap() = 0;

        //! \brief Check if the memory is compatible with an object
        //!
        //! The implementation is backend-specific, it not only checks if the size of device memory is greater
        //! or equal to the size of memory required by the object, but also checks backend's memory type, e.g.
        //! Vulkan's memory type bits to be compatible.
        //!
        //! \param pObject   - The object to check the memory for.
        //! \param sizeLimit - Maximum size of memory in bytes allowed for the object to occupy.
        //!
        //! \return True if the memory is compatible.
        [[nodiscard]] virtual bool IsCompatible(IDeviceObject* pObject, UInt64 sizeLimit) = 0;

        //! \brief Check if the memory is compatible with an object
        //!
        //! The implementation is backend-specific, it not only checks if the size of device memory is greater
        //! or equal to the size of memory required by the object, but also check backend's memory type, e.g.
        //! Vulkan's memory type bits to be compatible.
        //!
        //! \param pObject - The object to check the memory for.
        //!
        //! \return True if the memory is compatible.
        [[nodiscard]] virtual bool IsCompatible(IDeviceObject* pObject) = 0;
    };

    //! \brief A slice of device memory.
    class DeviceMemorySlice final
    {
        IDeviceMemory* m_pMemory = nullptr;
        UInt64 m_ByteOffset      = 0;
        UInt64 m_ByteSize        = 0;

    public:
        inline DeviceMemorySlice() = default;

        //! \brief Create a device memory slice.
        //!
        //! \param pMemory - The device memory to create a slice of.
        //! \param byteOffset  - The byte offset of the slice.
        //! \param byteSize    - The byte size of the slice.
        inline explicit DeviceMemorySlice(IDeviceMemory* pMemory, UInt64 byteOffset = 0,
                                          UInt64 byteSize = IDeviceMemory::WholeSize)
        {
            m_pMemory    = pMemory;
            m_ByteOffset = byteOffset;
            m_ByteSize   = std::min(byteSize, pMemory->GetDesc().Size - byteOffset);
            UN_Assert(byteSize == IDeviceMemory::WholeSize || byteSize <= pMemory->GetDesc().Size - byteOffset,
                      "Invalid memory slice size");
        }

        //! \brief Get the underlying device memory object.
        [[nodiscard]] inline IDeviceMemory* GetDeviceMemory() const
        {
            return m_pMemory;
        }

        //! \brief Get slice offset in bytes.
        [[nodiscard]] inline UInt64 GetByteOffset() const
        {
            return m_ByteOffset;
        }

        //! \brief Get slice size in bytes.
        [[nodiscard]] inline UInt64 GetByteSize() const
        {
            return m_ByteSize;
        }

        //! \brief Map the part of device memory represented by this slice.
        //!
        //! \param ppData - A pointer to the memory where the pointer to the mapped memory will be written.
        //!
        //! \return ResultCode::Success or an error node.
        [[nodiscard]] inline ResultCode Map(void** ppData) const
        {
            return m_pMemory->Map(m_ByteOffset, m_ByteSize, ppData);
        }

        //! \brief Map the part of device memory represented by this slice.
        //!
        //! \return The pointer to the mapped memory.
        [[nodiscard]] inline void* Map() const
        {
            void* pResult;
            if (auto result = m_pMemory->Map(m_ByteOffset, m_ByteSize, &pResult); Failed(result))
            {
                UN_Error(false, "Couldn't map memory, IDeviceMemory::Map returned {}", result);
                return nullptr;
            }

            return pResult;
        }

        //! \brief Map the part of device memory represented by this slice.
        //!
        //! \param byteOffset - Byte offset of the memory to map within this slice.
        //! \param byteSize   - Size of the part of the memory to map.
        //! \param ppData     - A pointer to the memory where the pointer to the mapped memory will be written.
        //!
        //! \return ResultCode::Success or an error node.
        [[nodiscard]] inline ResultCode Map(UInt64 byteOffset, UInt64 byteSize, void** ppData) const
        {
            UN_Assert(byteSize == IDeviceMemory::WholeSize || byteSize <= m_pMemory->GetDesc().Size - byteOffset,
                      "Invalid memory map size");

            return m_pMemory->Map(
                m_ByteOffset + byteOffset, std::min(byteSize, m_pMemory->GetDesc().Size - byteOffset - m_ByteOffset), ppData);
        }

        //! \brief Map the part of device memory represented by this slice.
        //!
        //! \param byteOffset - Byte offset of the memory to map within this slice.
        //! \param byteSize   - Size of the part of the memory to map.
        //!
        //! \return The pointer to the mapped memory.
        [[nodiscard]] inline void* Map(UInt64 byteOffset, UInt64 byteSize = IDeviceMemory::WholeSize) const
        {
            UN_Assert(byteSize == IDeviceMemory::WholeSize || byteSize <= m_pMemory->GetDesc().Size - byteOffset,
                      "Invalid memory map size");

            void* pResult;
            if (Succeeded(m_pMemory->Map(m_ByteOffset + byteOffset,
                                         std::min(byteSize, m_pMemory->GetDesc().Size - byteOffset - m_ByteOffset),
                                         &pResult)))
            {
                return pResult;
            }

            UN_Error(false, "Couldn't map memory");
            return nullptr;
        }

        //! \brief Unmap the mapped memory.
        //!
        //! \note This function does nothing if the memory was not mapped by calling Map().
        inline void Unmap() const
        {
            m_pMemory->Unmap();
        }

        //! \brief Check if the memory is compatible with an object
        //!
        //! The implementation is backend-specific, it not only checks if the size of device memory is greater
        //! or equal to the size of memory required by the object, but also check backend's memory type, e.g.
        //! Vulkan's memory type bits to be compatible.
        //!
        //! \param pObject - The object to check the memory for.
        //!
        //! \return True if the memory is compatible.
        [[nodiscard]] inline bool IsCompatible(IDeviceObject* pObject) const
        {
            return m_pMemory->IsCompatible(pObject, m_ByteSize);
        }
    };

    //! \brief Helper class for memory mapping, implements indexing operators and bound checking.
    template<class T>
    class MemoryMapHelper
    {
        DeviceMemorySlice m_MemorySlice;
        T* m_Map = nullptr;

        inline MemoryMapHelper() = default;

        inline explicit MemoryMapHelper(const DeviceMemorySlice& memorySlice, T* map)
            : m_MemorySlice(memorySlice)
            , m_Map(map)
        {
        }

    public:
        inline MemoryMapHelper(MemoryMapHelper&& other) noexcept
            : m_MemorySlice(other.m_MemorySlice)
            , m_Map(other.m_Map)
        {
            other.m_MemorySlice = {};
            other.m_Map         = nullptr;
        }

        inline ~MemoryMapHelper()
        {
            if (m_Map)
            {
                m_MemorySlice.Unmap();
            }
        }

        inline T& operator[](USize index)
        {
            UN_Assert(index * sizeof(T) < m_MemorySlice.GetByteSize(), "Index out of range in MemoryMapHelper");
            return m_Map[index];
        }

        //! \brief Get size of the mapped data as a number of elements of type T.
        [[nodiscard]] inline UInt64 Length() const
        {
            UN_Assert(IsValid(), "MemoryMapHelper was in invalid state");
            return m_MemorySlice.GetByteSize() / sizeof(T);
        }

        //! \brief Get size of the mapped data in bytes.
        [[nodiscard]] inline UInt64 ByteSize() const
        {
            UN_Assert(IsValid(), "MemoryMapHelper was in invalid state");
            return m_MemorySlice.GetByteSize();
        }

        //! \brief Check if an instance of the class is in valid state.
        [[nodiscard]] inline bool IsValid() const
        {
            return m_Map;
        }

        //! \brief Check if an instance of the class is in valid state.
        inline explicit operator bool() const
        {
            return IsValid();
        }

        //! \brief Get a pointer to the mapped memory.
        [[nodiscard]] inline T* Data() const
        {
            UN_Assert(IsValid(), "MemoryMapHelper was in invalid state");
            return m_Map;
        }

        //! \brief Map the device memory slice.
        //!
        //! The function returns an instance of MemoryMapHelper that can be invalid if the memory
        //! map operation did not succeed. Check it like this:
        //!
        //! \code{.cpp}
        //! if (auto mapHelper = MemoryMapHelper<float>::Map(...))
        //! {
        //!     // Use mapHelper here...
        //! }
        //! \endcode
        //!
        //! \param memorySlice - The device memory slice to map.
        //!
        //! \return An instance of MemoryMapHelper.
        inline static MemoryMapHelper Map(const DeviceMemorySlice& memorySlice)
        {
            void* map;
            auto result = memorySlice.Map(&map);
            if (Succeeded(result))
            {
                return MemoryMapHelper(memorySlice, static_cast<T*>(map));
            }

            return {};
        }

        //! \brief Map the device memory.
        //!
        //! The function returns an instance of MemoryMapHelper that can be invalid if the memory
        //! map operation did not succeed. Check it like this:
        //!
        //! \code{.cpp}
        //! if (auto mapHelper = MemoryMapHelper<float>::Map(...))
        //! {
        //!     // Use mapHelper here...
        //! }
        //! \endcode
        //!
        //! \param pMemory    - The device memory to map.
        //! \param byteOffset - Byte offset of the memory to map.
        //! \param byteSize   - Size of the part of the memory to map.
        //!
        //! \return An instance of MemoryMapHelper.
        inline static MemoryMapHelper Map(IDeviceMemory* pMemory, UInt64 byteOffset = 0,
                                          UInt64 byteSize = IDeviceMemory::WholeSize)
        {
            return Map(DeviceMemorySlice(pMemory, byteOffset, byteSize));
        }
    };
} // namespace UN
