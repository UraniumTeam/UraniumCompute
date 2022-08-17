#pragma once
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
        InvalidArguments, //!< One or more arguments were invalid.
        AccessDenied,     //!< General access denied error occurred.
        Timeout,          //!< Operation timed out.
        OutOfMemory       //!< Not enough memory to complete the operation.
    };

    //! \brief Macro to check if an operation returned ResultCode::Success.
#define UN_SUCCEEDED(result) ((result) == ::UN::ResultCode::Success)
} // namespace UN
