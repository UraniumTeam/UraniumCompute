#pragma once
#include <UnCompute/Containers/ArraySlice.h>
#include <UnCompute/Memory/IAllocator.h>
#include <UnCompute/Memory/SystemAllocator.h>

namespace UN
{
    template<class T>
    class HeapArray final
    {
        ArraySlice<T> m_Storage{};
        IAllocator* m_pAllocator{};

        inline void AllocateStorage(USize count)
        {
            m_Storage = ArraySlice<T>(m_pAllocator->Allocate(count * sizeof(T), alignof(T)), count);
        }

        inline void AllocateStorage(USize count, const T& value)
        {
            m_Storage = ArraySlice<T>(m_pAllocator->Allocate(count * sizeof(T), alignof(T)), count);

            for (USize i = 0; i < count; ++i)
            {
                m_Storage[i] = value;
            }
        }

        inline void DeallocateStorage(const ArraySlice<T>& storage)
        {
            if (storage.Empty())
            {
                return;
            }

            m_pAllocator->Deallocate(storage.Data());
            storage = {};
        }

        inline void DeallocateStorage()
        {
            DeallocateStorage(m_Storage);
        }

    public:
        //! \brief Create an empty array
        inline HeapArray()
            : m_pAllocator(SystemAllocator::Get())
        {
        }

        //! \brief Copy constructor.
        inline HeapArray(const HeapArray& other)
            : m_pAllocator(other.m_pAllocator)
        {
            AllocateStorage(other.Length());
            other.CopyDataTo(m_Storage);
        }

        //! \brief Move constructor.
        inline HeapArray(HeapArray&& other) noexcept
            : m_Storage(other.m_Storage)
            , m_pAllocator(other.m_pAllocator)
        {
            other.m_Storage = {};
        }

        //! \brief Copy assignment.
        inline HeapArray& operator=(const HeapArray& other)
        {
            if (this == &other)
            {
                return *this;
            }

            DeallocateStorage();
            AllocateStorage(other.Length());
            other.CopyDataTo(m_Storage);

            return *this;
        }

        //! \brief Move assignment.
        inline HeapArray& operator=(HeapArray&& other) noexcept
        {
            DeallocateStorage();
            m_Storage       = other.m_Storage;
            m_pAllocator    = other.m_pAllocator;
            other.m_Storage = {};
            return *this;
        }

        inline ~HeapArray()
        {
            DeallocateStorage();
        }

        //! \brief Create an array.
        //!
        //! \param pAllocator - The allocator to use for heap allocations.
        inline explicit HeapArray(IAllocator* pAllocator)
            : m_pAllocator(pAllocator)
        {
        }

        //! \brief Create an array.
        //!
        //! \param size  - The number of array elements.
        //! \param value - The value to set to each element.
        inline explicit HeapArray(USize size, const T& value = {})
            : m_pAllocator(SystemAllocator::Get())
        {
            AllocateStorage(size, value);
        }

        //! \brief Create an array.
        //!
        //! \param pAllocator - The allocator to use for heap allocations.
        //! \param size       - The number of array elements.
        //! \param value      - The value to set to each element.
        inline HeapArray(IAllocator* pAllocator, USize size, const T& value = {})
            : m_pAllocator(pAllocator)
        {
            AllocateStorage(size, value);
        }

        //! \brief Set size of the array.
        //!
        //! \param size  - The number of array elements.
        //! \param value - The value to set to each new element.
        inline void Resize(USize size, const T& value = {})
        {
            ArraySlice<T> temp = m_Storage;
            AllocateStorage(size);
            temp.CopyDataTo(m_Storage);
            for (USize i = temp.Length(); i < Length(); ++i)
            {
                m_Storage[i] = temp[i];
            }
            DeallocateStorage(temp);
        }

        //! \brief Create a subslice from this array.
        //!
        //! \param beginIndex - The index of the first element.
        //! \param endIndex   - The index of the first element after the end.
        //!
        //! \return The created subslice.
        inline ArraySlice<T> operator()(USize beginIndex, USize endIndex) const noexcept
        {
            return m_Storage(beginIndex, endIndex);
        }

        //! \brief Get an element by index.
        //!
        //! \param index - The index of the element to get.
        inline T& operator[](USize index) const noexcept
        {
            return m_Storage[index];
        }

        //! \brief Get index of the first element equal to the passed value.
        //!
        //! \param value - The value to look for.
        //!
        //! \return The index of the value or -1.
        [[nodiscard]] inline SSize IndexOf(const T& value) const
        {
            return m_Storage.IndexOf(value);
        }

        //! \brief Get index of the last element equal to the passed value.
        //!
        //! \param value - The value to look for.
        //!
        //! \return The index of the value or -1.
        [[nodiscard]] inline SSize LastIndexOf(const T& value) const
        {
            return m_Storage.LastIndexOf(value);
        }

        //! \brief Check if the element is present in the array.
        //!
        //! \param value - The value to look for.
        [[nodiscard]] inline bool Contains(const T& value)
        {
            return IndexOf(value) != -1;
        }

        //! \bried Length of the array.
        [[nodiscard]] inline USize Length() const
        {
            return m_Storage.Length();
        }

        //! \brief Check if the array is empty.
        [[nodiscard]] inline bool Empty() const
        {
            return Length() == 0;
        }

        //! \brief Check if the array has any elements.
        [[nodiscard]] inline bool Any() const
        {
            return Length() > 0;
        }

        //! \brief Reset the array to empty state.
        inline void Reset()
        {
            DeallocateStorage();
        }

        //! \brief Get pointer to the beginning of the array.
        [[nodiscard]] inline const T* Data() const
        {
            return m_Storage.Data();
        }

        //! \brief Copy data from this array to a slice.
        //!
        //! \param destination - The destination slice.
        //!
        //! \return The number of actually copied elements.
        inline USize CopyDataTo(ArraySlice<T> destination) const
        {
            return m_Storage.CopyDataTo(destination);
        }

        [[nodiscard]] inline const T* begin() const
        {
            return m_Storage.begin();
        }

        [[nodiscard]] inline const T* end() const
        {
            return m_Storage.end();
        }

        inline operator ArraySlice<T>() const // NOLINT(google-explicit-constructor)
        {
            return m_Storage;
        }
    };
} // namespace UN
