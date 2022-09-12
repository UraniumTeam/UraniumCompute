#pragma once
#include <UnCompute/Base/Flags.h>
#include <iomanip>

namespace UN
{
    //! \brief A type-safe integral with size of a single byte, used to store raw memory as ArraySlice<T> or HeapArray<T>.
    enum class Byte : UInt8
    {
    };

    static_assert(sizeof(Byte) == 1);

    template<class T>
    inline std::enable_if_t<std::is_integral_v<T>, Byte*> un_byte_cast(T* ptr)
    {
        return reinterpret_cast<Byte*>(ptr);
    }

    template<class T>
    inline std::enable_if_t<std::is_integral_v<T>, const Byte*> un_byte_cast(const T* ptr)
    {
        return reinterpret_cast<const Byte*>(ptr);
    }

    template<class TOStream>
    inline TOStream& operator<<(TOStream& stream, Byte byte)
    {
        return stream << "0x" << std::setfill('0') << std::setw(2) << std::right << std::hex << static_cast<Int32>(byte);
    }
} // namespace UN
