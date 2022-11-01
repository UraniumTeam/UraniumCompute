#pragma once
#include <csignal>
#include <stdexcept>

#if defined NDEBUG
#    define UN_RELEASE 1
#else
#    define UN_DEBUG 1
#endif

#define UN_MAKE_STR(txt) #txt

//! \brief Mark an unused parameter to suppress compiler warning.
#define UN_UNUSED(expr) (void)(expr)

#if defined __clang__
#    define UN_COMPILER_CLANG 1

#    if defined _MSC_VER
#        define UN_COMPILER_MS_CLANG 1
#    endif

#    define UN_PUSH_MSVC_WARNING(...)
#    define UN_POP_MSVC_WARNING

#    define UN_PUSH_CLANG_WARNING(warn) _Pragma("clang diagnostic push") _Pragma(UN_MAKE_STR(clang diagnostic ignored warn))
#    define UN_POP_CLANG_WARNING _Pragma("clang diagnostic pop")

#    define UN_FUNCSIG __PRETTY_FUNCTION__

#    ifndef UN_FINLINE
#        define UN_FINLINE inline
#    endif
#elif defined _MSC_VER || defined DOXYGEN
#    define UN_COMPILER_MSVC 1

#    define UN_PUSH_MSVC_WARNING(warn) __pragma(warning(push)) __pragma(warning(disable : warn))
#    define UN_POP_MSVC_WARNING __pragma(warning(pop))

#    define UN_PUSH_CLANG_WARNING(...)
#    define UN_POP_CLANG_WARNING

//! \brief Function signature.
#    define UN_FUNCSIG __FUNCSIG__

#    ifndef UN_FINLINE
#        define UN_FINLINE __forceinline
#    endif
#endif

#if UN_COMPILER_MSVC || UN_COMPILER_MS_CLANG || defined DOXYGEN
//! \brief Break debugger at runtime.
#    define UN_DebugBreak() __debugbreak()
#else
#    define UN_DebugBreak() raise(SIGABRT)
#endif
