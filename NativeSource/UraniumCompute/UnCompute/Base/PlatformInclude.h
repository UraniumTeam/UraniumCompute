#pragma once
#include <UnCompute/Base/Platform.h>

#if UN_WINDOWS
#    define NOMINMAX
#    define WIN32_LEAN_AND_MEAN
#    include <Windows.h>

#    include <atlbase.h>
#    include <atlcom.h>
#    include <guiddef.h>

#    undef CopyMemory
#    undef GetObject
#    undef CreateWindow
#    undef MemoryBarrier
#else
#   include <dlfcn.h>
#endif
