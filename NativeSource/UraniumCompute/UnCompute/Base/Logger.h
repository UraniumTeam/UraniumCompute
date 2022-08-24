#pragma once
#include <UnCompute/Base/CompilerTraits.h>
#include <UnCompute/Base/ResultCode.h>

#if UN_DEBUG
#    define SPDLOG_ACTIVE_LEVEL SPDLOG_LEVEL_DEBUG

//! \brief Unconditionally stop the program execution, will break the attached debugger if available.
#    define UN_Fail()                                                                                                            \
        do                                                                                                                       \
        {                                                                                                                        \
            UN_DebugBreak();                                                                                                     \
        }                                                                                                                        \
        while (false)
#else
#    define SPDLOG_ACTIVE_LEVEL SPDLOG_LEVEL_INFO

//! \brief Unconditionally stop the program execution, will break the attached debugger if available.
#    define UN_Fail()                                                                                                            \
        do                                                                                                                       \
        {                                                                                                                        \
            assert(false && "Fatal error");                                                                                      \
        }                                                                                                                        \
        while (false)
#endif

#include <spdlog/spdlog.h>
#include <string_view>

template<>
struct fmt::formatter<UN::ResultCode> : fmt::formatter<std::string_view>
{
    template<typename FormatContext>
    auto format(const UN::ResultCode& result, FormatContext& ctx) const -> decltype(ctx.out())
    {
        return fmt::format_to(ctx.out(), "{}", UN::ResultToString(result));
    }
};

//! \brief Format and log a message.
//!
//! Currently the library uses spdlog logger. See the documentation of spdlog for more information about message formatting.
//!
//! @{
#define UNLOG_Debug(...) SPDLOG_DEBUG(__VA_ARGS__)
#define UNLOG_Info(...) SPDLOG_INFO(__VA_ARGS__)
#define UNLOG_Warning(...) SPDLOG_WARN(__VA_ARGS__)
#define UNLOG_Error(...) SPDLOG_ERROR(__VA_ARGS__)
//! @}

//! \brief Verify expression, will use UN_Fail() to stop the program in debug and release builds.
#define UN_Verify(expr, ...)                                                                                                     \
    do                                                                                                                           \
    {                                                                                                                            \
        if (!(expr))                                                                                                             \
        {                                                                                                                        \
            UNLOG_Error(__VA_ARGS__);                                                                                            \
            UN_Fail();                                                                                                           \
        }                                                                                                                        \
    }                                                                                                                            \
    while (false)

//! \brief Verify that ResultCode succeeded, will use UN_Fail() to stop the program in debug and release builds.
//!
//! \see UN::Succeeded
#define UN_VerifyResult(expr, ...) UN_Verify(::UN::Succeeded(expr), __VA_ARGS__)

//! \brief Verify and expression and log a warning, works in release builds too.
#define UN_VerifyWarning(expr, ...)                                                                                              \
    do                                                                                                                           \
    {                                                                                                                            \
        if (!(expr))                                                                                                             \
        {                                                                                                                        \
            UNLOG_Warning(__VA_ARGS__);                                                                                          \
        }                                                                                                                        \
    }                                                                                                                            \
    while (false)

//! \brief Verify and expression and log an error, works in release builds too.
#define UN_VerifyError(expr, ...)                                                                                                \
    do                                                                                                                           \
    {                                                                                                                            \
        if (!(expr))                                                                                                             \
        {                                                                                                                        \
            UNLOG_Error(__VA_ARGS__);                                                                                            \
        }                                                                                                                        \
    }                                                                                                                            \
    while (false)

#if UN_DEBUG
//! \brief Same as UN_Verify, but works only in debug builds.
#    define UN_Assert(expr, ...) UN_Verify(expr, __VA_ARGS__)
//! \brief Same as UN_VerifyWarning, but works only in debug builds.
#    define UN_Warning(expr, ...) UN_VerifyWarning(expr, __VA_ARGS__)
//! \brief Same as UN_VerifyError, but works only in debug builds.
#    define UN_Error(expr, ...) UN_VerifyError(expr, __VA_ARGS__)
#else
//! \brief Same as UN_Verify, but works only in debug builds.
#    define UN_Assert(expr, ...) UN_UNUSED(expr)
//! \brief Same as UN_VerifyWarning, but works only in debug builds.
#    define UN_Warning(expr, ...) UN_UNUSED(expr)
//! \brief Same as UN_VerifyError, but works only in debug builds.
#    define UN_Error(expr, ...) UN_UNUSED(expr)
#endif

namespace UN
{
    //! \brief Set up the logging library.
    inline void InitializeLogger()
    {
        spdlog::set_level(spdlog::level::debug);
    }
} // namespace UN
