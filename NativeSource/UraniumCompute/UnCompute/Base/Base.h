#pragma once
#include <UnCompute/Base/Logger.h>
#include <UnCompute/Base/Platform.h>
#include <UnCompute/Base/ResultCode.h>
#include <atomic>
#include <cassert>
#include <cstdint>
#include <intrin.h>
#include <string_view>

namespace UN
{
    //! \brief Numeric type aliases
    //! @{
    using Int8  = int8_t;
    using Int16 = int16_t;
    using Int32 = int32_t;
    using Int64 = int64_t;

    using UInt8  = uint8_t;
    using UInt16 = uint16_t;
    using UInt32 = uint32_t;
    using UInt64 = uint64_t;

    using Float32 = float;
    using Float64 = double;
    //! @}

    using USize = size_t;    //!< Unsigned integer with the size of a pointer on the current platform.
    using SSize = ptrdiff_t; //!< Signed integer with the size of a pointer on the current platform.

    //! \brief C#-compatible type aliases
    //! @{
    using CsSByte = Int8;
    using CsShort = Int16;
    using CsInt   = Int32;
    using CsLong  = Int64;

    using CsByte   = UInt8;
    using CsUShort = UInt16;
    using CsUInt   = UInt32;
    using CsULong  = UInt64;
    //! @}

    inline constexpr struct
    {
        int Major = 0, Minor = 1, Patch = 0;
    } UnComputeVersion; //!< The version of the library.

    //! \internal
    namespace Internal
    {
        //! \brief A simple `std::string_view` wrapper.
        //!
        //! This is useful for function signatures when compiling with MSVC. `std::string_view` is a template class
        //! (`std::basic_string_view< ... >`). It makes difficult to retrieve typename from a template function
        //! signature since it makes more than one template.
        struct SVWrapper
        {
            std::string_view value; //!< Actual value of the string view.
        };

        //! \brief Remove leading and trailing spaces from a string view.
        inline constexpr std::string_view TrimTypeName(std::string_view name)
        {
            name.remove_prefix(name.find_first_not_of(' '));
            name.remove_suffix(name.length() - name.find_last_not_of(' ') - 1);
            return name;
        }

        //! \brief Internal implementation of \ref UN::TypeName.
        template<class T>
        inline constexpr SVWrapper TypeNameImpl()
        {
#if UN_COMPILER_MSVC
            std::string_view fn = __FUNCSIG__;
            fn.remove_prefix(fn.find_first_of("<") + 1);
            fn.remove_suffix(fn.length() - fn.find_last_of(">"));
#else
            std::string_view fn = __PRETTY_FUNCTION__;
            fn.remove_prefix(fn.find_first_of('=') + 1);
            fn.remove_suffix(fn.length() - fn.find_last_of(']'));
#endif
            return SVWrapper{ TrimTypeName(fn) };
        }
    } // namespace Internal
    //! \endinternal

    //! \brief Get name of a type as a compile-time constant.
    //!
    //! This implementation uses the `__PRETTY_FUNCTION__` hack to retrieve typename from a function signature
    //! at compile-time.
    template<class T>
    inline constexpr std::string_view TypeName()
    {
        return Internal::TypeNameImpl<T>().value;
    }

    //! \brief Maximum alignment value, should be enough for anything
    inline constexpr USize MaximumAlignment = 16;

    //! \brief Align up an integer.
    //!
    //! \param x     - Value to align.
    //! \param align - Alignment to use.
    template<class T, class U = T>
    inline T AlignUp(T x, U align)
    {
        return (x + (align - 1u)) & ~(align - 1u);
    }

    //! \brief Align up a pointer.
    //!
    //! \param x     - Value to align.
    //! \param align - Alignment to use.
    template<class T>
    inline T* AlignUpPtr(const T* x, USize align)
    {
        return reinterpret_cast<T*>(AlignUp(reinterpret_cast<USize>(x), align));
    }

    //! \brief Align up an integer.
    //!
    //! \param x     - Value to align.
    //! \tparam A         - Alignment to use.
    template<UInt32 A, class T>
    inline constexpr T AlignUp(T x)
    {
        return (x + (A - 1)) & ~(A - 1);
    }

    //! \brief Align down an integer.
    //!
    //! \param x     - Value to align.
    //! \param align - Alignment to use.
    template<class T, class U = T>
    inline T AlignDown(T x, U align)
    {
        return (x & ~(align - 1));
    }

    //! \brief Align down a pointer.
    //!
    //! \param x     - Value to align.
    //! \param align - Alignment to use.
    template<class T>
    inline constexpr T* AlignDownPtr(const T* x, USize align)
    {
        return reinterpret_cast<T*>(AlignDown(reinterpret_cast<USize>(x), align));
    }

    //! \brief Align down an integer.
    //!
    //! \param x  - Value to align.
    //! \tparam A - Alignment to use.
    template<UInt32 A, class T>
    inline constexpr T AlignDown(T x)
    {
        return (x & ~(A - 1));
    }

    //! \brief Create a bit mask.
    //!
    //! \param bitCount  - Number of bits in the mask.
    //! \param leftShift - Number of zeros on the left side of the mask.
    template<class T>
    inline constexpr T MakeMask(T bitCount, T leftShift)
    {
        auto typeBitCount = sizeof(T) * 8;
        auto mask         = bitCount == typeBitCount ? static_cast<T>(-1) : ((1 << bitCount) - 1);
        return static_cast<T>(mask << leftShift);
    }

    inline void HashCombine(std::size_t& /* seed */) {}

    //! \brief Combine hashes of specified values with seed.
    //!
    //! \tparam Args - Types of values.
    //!
    //! \param seed - Initial hash value to combine with.
    //! \param args - The values to calculate hash of.
    template<typename T, typename... Args>
    inline void HashCombine(std::size_t& seed, const T& value, const Args&... args)
    {
        std::hash<T> hasher;
        seed ^= hasher(value) + 0x9e3779b9 + (seed << 6) + (seed >> 2);
        HashCombine(seed, args...);
    }

    //! \brief Define std::hash<> for a type.
#define UN_MAKE_HASHABLE(TypeName, Template, ...)                                                                                \
    namespace std                                                                                                                \
    {                                                                                                                            \
        template<Template>                                                                                                       \
        struct hash<TypeName>                                                                                                    \
        {                                                                                                                        \
            inline size_t operator()(const TypeName& value) const noexcept                                                       \
            {                                                                                                                    \
                size_t seed = 0;                                                                                                 \
                ::UN::HashCombine(seed, __VA_ARGS__);                                                                            \
                return seed;                                                                                                     \
            }                                                                                                                    \
        };                                                                                                                       \
    }

#if UN_DEBUG
    //! \brief Is true in debug builds.
    inline constexpr bool ValidationEnabled = true;
#else
    //! \brief Is true in debug builds.
    inline constexpr bool ValidationEnabled = false;
#endif

    //! \brief Cast from base to derived or crash in debug build.
    //!
    //! This function uses `UN_Assert` to verify that that the base pointer can be casted to derived pointer in debug builds.
    //! In release builds it behaves like standard `static_cast`.
    //!
    //! \tparam TDest - Destination pointer type.
    //! \tparam TSrc  - Source pointer type.
    //!
    //! \param pSourceObject - The pointer to cast.
    //!
    //! \return The result pointer.
    template<class TDest, class TSrc>
    inline std::enable_if_t<std::is_base_of_v<TSrc, TDest>, TDest*> un_verify_cast(TSrc* pSourceObject)
    {
        if constexpr (ValidationEnabled) // NOLINT
        {
            if (auto* result = dynamic_cast<TDest*>(pSourceObject))
            {
                return result;
            }

            UN_Assert(false, "Verifying cast failed");
        }

        return static_cast<TDest*>(pSourceObject);
    }
} // namespace UN
