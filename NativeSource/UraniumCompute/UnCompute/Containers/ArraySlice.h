#pragma once
#include <UnCompute/Base/Base.h>
#include <array>
#include <cstring>
#include <vector>

namespace UN
{
    //! \brief This class represents a non-owning slice of contiguously stored elements.
    template<class T>
    class ArraySlice final
    {
        T* m_pBegin = nullptr;
        T* m_pEnd   = nullptr;

        template<class T1>
        friend class ArraySlice;

        template<class TCont>
        using AddConst = std::conditional_t<std::is_const_v<T>, const TCont, TCont>;

        using StdVector = AddConst<std::vector<std::remove_const_t<T>>>;
        template<USize N>
        using StdArray = AddConst<std::array<std::remove_const_t<T>, N>>;

    public:
        inline ArraySlice() = default;

        //! \brief Create an array slice.
        //!
        //! \param pData - The pointer to the first element.
        //! \param count - The number of elements.
        inline ArraySlice(T* pData, USize count) noexcept
            : m_pBegin(pData)
            , m_pEnd(pData + count)
        {
        }

        //! \brief Create an array slice.
        //!
        //! \param pBegin - The pointer to the first element.
        //! \param pEnd   - The pointer to the first element after the end.
        inline ArraySlice(T* pBegin, T* pEnd) noexcept
            : m_pBegin(pBegin)
            , m_pEnd(pEnd)
        {
        }

        //! \brief Create an array slice.
        //!
        //! \tparam N - The number of elements.
        //!
        //! \param arr - The source array.
        template<USize N>
        inline ArraySlice(T (&arr)[N]) noexcept
            : m_pBegin(arr)
            , m_pEnd(arr + N)
        {
        }

        //! \brief Create an array slice.
        //!
        //! \param vector - The vector to create the slice from.
        inline ArraySlice(StdVector& vector) noexcept // NOLINT
            : m_pBegin(vector.data())
            , m_pEnd(vector.data() + vector.size())
        {
        }

        //! \brief Create an array slice.
        //!
        //! \tparam N - The number of elements.
        //!
        //! \param array - The array to create the slice from.
        template<USize N>
        inline ArraySlice(StdArray<N>& array) noexcept // NOLINT
            : m_pBegin(array.data())
            , m_pEnd(array.data() + array.size())
        {
        }

        //! \brief Create an array slice.
        //!
        //! \param lst - The initializer list to create the slice from.
        //!
        //! \note The created instance will only be valid if passed as a parameter.
        inline ArraySlice(std::initializer_list<T> lst) noexcept
            : m_pBegin(lst.begin())
            , m_pEnd(lst.end())
        {
        }

        //! \brief Create a subslice from this array slice.
        //!
        //! \param beginIndex - The index of the first element.
        //! \param endIndex   - The index of the first element after the end.
        //!
        //! \return The created subslice.
        inline ArraySlice operator()(USize beginIndex, USize endIndex) const
        {
            UN_Assert(beginIndex < Length() && endIndex <= Length(), "Index out of range");
            return ArraySlice(m_pBegin + beginIndex, m_pBegin + endIndex);
        }

        //! \brief Get an element by index.
        //!
        //! \param index - The index of the element to get.
        inline T& operator[](USize index) const
        {
            UN_Assert(index < Length(), "Index out of range");
            return m_pBegin[index];
        }

        //! \brief Get index of the first element equal to the passed value.
        //!
        //! \param value - The value to look for.
        //!
        //! \return The index of the value or -1.
        [[nodiscard]] inline SSize IndexOf(const T& value) const noexcept
        {
            auto length = static_cast<SSize>(Length());
            for (SSize i = 0; i < length; ++i)
            {
                if (value == m_pBegin[i])
                {
                    return i;
                }
            }

            return -1;
        }

        //! \brief Get index of the last element equal to the passed value.
        //!
        //! \param value - The value to look for.
        //!
        //! \return The index of the value or -1.
        [[nodiscard]] inline SSize LastIndexOf(const T& value) const noexcept
        {
            auto length = static_cast<SSize>(Length());
            for (SSize i = length - 1; i >= 0; --i)
            {
                if (value == m_pBegin[i])
                {
                    return i;
                }
            }

            return -1;
        }

        //! \brief Check if the element is present in the slice.
        //!
        //! \param value - The value to look for.
        [[nodiscard]] inline bool Contains(const T& value) const noexcept
        {
            return IndexOf(value) != -1;
        }

        //! \bried Length of the slice.
        [[nodiscard]] inline USize Length() const noexcept
        {
            return m_pEnd - m_pBegin;
        }

        //! \brief Check if the slice is empty.
        [[nodiscard]] inline bool Empty() const noexcept
        {
            return Length() == 0;
        }

        //! \brief Check if the slice has any elements.
        [[nodiscard]] inline bool Any() const noexcept
        {
            return Length() > 0;
        }

        //! \brief Reset the slice to empty state.
        inline void Reset() noexcept
        {
            m_pBegin = m_pEnd = nullptr;
        }

        //! \brief Get pointer to the beginning of the slice.
        [[nodiscard]] inline T* Data() const noexcept
        {
            return m_pBegin;
        }

        //! \brief Copy data from this slice to another.
        //!
        //! \param destination - The destination slice.
        //!
        //! \return The number of actually copied elements.
        inline USize CopyDataTo(ArraySlice<std::remove_const_t<T>> destination) const
        {
            USize size = std::min(Length(), destination.Length());

            if constexpr (std::is_trivially_copyable_v<T>)
            {
                memcpy(destination.Data(), Data(), size * sizeof(T));
            }
            else
            {
                for (USize i = 0; i < size; ++i)
                {
                    destination.m_pBegin[i] = m_pBegin[i];
                }
            }

            return size;
        }

        template<class T1>
        inline ArraySlice<T1> ReinterpretAs() const
        {
            ArraySlice<T1> result;
            result.m_pBegin = reinterpret_cast<T1*>(m_pBegin);
            result.m_pEnd   = reinterpret_cast<T1*>(m_pEnd);
            return result;
        }

        [[nodiscard]] inline const T* begin() const noexcept
        {
            return m_pBegin;
        }

        [[nodiscard]] inline const T* end() const noexcept
        {
            return m_pEnd;
        }

        inline operator ArraySlice<std::add_const_t<T>>() const noexcept // NOLINT(google-explicit-constructor)
        {
            return { Data(), Data() + Length() };
        }

        inline friend bool operator==(const ArraySlice& lhs, const ArraySlice& rhs) noexcept
        {
            return lhs.m_pBegin == rhs.m_pBegin && lhs.m_pEnd == rhs.m_pEnd;
        }

        inline friend bool operator!=(const ArraySlice& lhs, const ArraySlice& rhs) noexcept
        {
            return lhs.m_pBegin != rhs.m_pBegin || lhs.m_pEnd != rhs.m_pEnd;
        }
    };
} // namespace UN
