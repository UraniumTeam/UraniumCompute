#pragma once
#include <UnCompute/Base/Base.h>

namespace UN
{
//! \brief Create a single-bit literal.
#define UN_BIT(n) (1 << (n))

//! \brief Define bitwise operations on `enum`.
//!
//! The macro defines bitwise or, and, xor operators.
#define UN_ENUM_OPERATORS(Name)                                                                                                  \
    inline constexpr Name operator|(Name a, Name b)                                                                              \
    {                                                                                                                            \
        return Name(((std::underlying_type_t<Name>)a) | ((std::underlying_type_t<Name>)b));                                      \
    }                                                                                                                            \
    inline constexpr Name& operator|=(Name& a, Name b)                                                                           \
    {                                                                                                                            \
        return a = a | b;                                                                                                        \
    }                                                                                                                            \
    inline constexpr Name operator&(Name a, Name b)                                                                              \
    {                                                                                                                            \
        return Name(((std::underlying_type_t<Name>)a) & ((std::underlying_type_t<Name>)b));                                      \
    }                                                                                                                            \
    inline constexpr Name& operator&=(Name& a, Name b)                                                                           \
    {                                                                                                                            \
        return a = a & b;                                                                                                        \
    }                                                                                                                            \
    inline constexpr Name operator^(Name a, Name b)                                                                              \
    {                                                                                                                            \
        return Name(((std::underlying_type_t<Name>)a) ^ ((std::underlying_type_t<Name>)b));                                      \
    }                                                                                                                            \
    inline constexpr Name& operator^=(Name& a, Name b)                                                                           \
    {                                                                                                                            \
        return a = a ^ b;                                                                                                        \
    }

    //! \internal
    namespace Internal
    {
        template<class T>
        inline constexpr bool AllowedAsFlags = std::is_enum_v<T> | std::is_integral_v<T>;
    }

    //! \brief Check if any of flags match.
    template<class TFlags>
    inline constexpr std::enable_if_t<Internal::AllowedAsFlags<TFlags>, bool> FlagsAny(TFlags source, TFlags test)
    {
        return (source & test) != static_cast<TFlags>(0);
    }

    //! \brief Check if none of flags match.
    template<class TFlags>
    inline constexpr std::enable_if_t<Internal::AllowedAsFlags<TFlags>, bool> FlagsNone(TFlags source, TFlags test)
    {
        return (source & test) == static_cast<TFlags>(0);
    }

    //! \brief Check if all of flags match.
    template<class TFlags>
    inline constexpr std::enable_if_t<Internal::AllowedAsFlags<TFlags>, bool> FlagsAll(TFlags source, TFlags test)
    {
        return (source ^ test) == static_cast<TFlags>(0);
    }

    //! \brief Remove some bits from flags.
    template<class TFlags>
    inline constexpr std::enable_if_t<std::is_enum_v<TFlags>, TFlags> FlagsRemove(TFlags source, TFlags remove)
    {
        return source & static_cast<TFlags>(~static_cast<std::underlying_type_t<TFlags>>(remove));
    }

    //! \brief Remove some bits from flags.
    template<class TFlags>
    inline constexpr std::enable_if_t<std::is_integral_v<TFlags>, TFlags> FlagsRemove(TFlags source, TFlags remove)
    {
        return source & (~remove);
    }
} // namespace UN
