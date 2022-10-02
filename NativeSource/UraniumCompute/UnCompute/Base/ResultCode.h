#pragma once
#include <cassert>
#include <cstdint>

namespace UN
{
    //! \brief Represents a general result of a function call within the library.
    //!
    //! Different functions may have their own result codes, but this enum should be enough for a general case.
    enum class ResultCode : uint32_t
    {
        Success,          //!< Operation succeeded.
        Fail,             //!< Operation failed.
        Abort,            //!< Operation aborted.
        NotImplemented,   //!< Operation not implemented.
        InvalidOperation, //!< Operation was invalid.
        InvalidArguments, //!< One or more arguments were invalid.
        AccessDenied,     //!< General access denied error occurred.
        Timeout,          //!< Operation timed out.
        OutOfMemory       //!< Not enough memory to complete the operation.
    };

    //! \brief Check if an operation returned ResultCode::Success.
    inline bool Succeeded(ResultCode result)
    {
        return result == ResultCode::Success;
    }

    //! \brief Check if an operation did not return ResultCode::Success.
    inline bool Failed(ResultCode result)
    {
        return result != ResultCode::Success;
    }

    inline const char* ResultToString(ResultCode result)
    {
        switch (result)
        {
            // clang-format off
        case ResultCode::Success: return "ResultCode::Success";
        case ResultCode::Fail: return "ResultCode::Fail";
        case ResultCode::Abort: return "ResultCode::Abort";
        case ResultCode::NotImplemented: return "ResultCode::NotImplemented";
        case ResultCode::InvalidArguments: return "ResultCode::InvalidArguments";
        case ResultCode::InvalidOperation: return "ResultCode::InvalidOperation";
        case ResultCode::AccessDenied: return "ResultCode::AccessDenied";
        case ResultCode::Timeout: return "ResultCode::Timeout";
        case ResultCode::OutOfMemory: return "ResultCode::OutOfMemory";
            // clang-format on
        default:
            assert(false && "ResultCode was unknown");
            return "ResultCode::<Unknown>";
        }
    }
} // namespace UN
