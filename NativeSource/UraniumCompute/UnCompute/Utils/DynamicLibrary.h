#pragma once
#include <UnCompute/Base/PlatformInclude.h>
#include <UnCompute/Memory/Memory.h>

#if UN_WINDOWS
#    define UN_LoadLibrary(name) LoadLibraryA(name)
#    define UN_GetFunctionAddress(handle, name) reinterpret_cast<void*>(GetProcAddress(static_cast<HMODULE>(handle), name))
#    define UN_FreeLibrary(handle) FreeLibrary(static_cast<HMODULE>(handle))
#else
#    define UN_LoadLibrary(name) dlopen(name, RTLD_LAZY)
#    define UN_GetFunctionAddress(handle, name) dlsym(handle, name)
#    define UN_FreeLibrary(handle) dlclose(handle)
#endif

namespace UN
{
    //! \brief A class for loading and unloading DLLs.
    class DynamicLibrary : public Object<IObject>
    {
        void* m_NativeHandle = nullptr;

        void* GetFunctionImpl(const char* functionName);

    public:
        inline DynamicLibrary() = default;
        ~DynamicLibrary() override;

        DynamicLibrary(const DynamicLibrary&)            = delete;
        DynamicLibrary& operator=(const DynamicLibrary&) = delete;
        DynamicLibrary(DynamicLibrary&&)                 = delete;
        DynamicLibrary& operator=(DynamicLibrary&&)      = delete;

        //! \brief Load a library with specified name.
        //!
        //! \param name - Dynamic library name.
        //!
        //! \return ResultCode::Success or an error code.
        ResultCode Init(std::string_view name);

        //! \brief Unload the library.
        void Unload();

        //! \brief Load a function entry point from the DLL.
        //!
        //! \tparam FPtr - Type of the function pointer to return.
        //!
        //! \param functionName - The name of the function to load.
        //! \param pResult      - A pointer to memory where the result will be written.
        //!
        //! \return ResultCode::Success or an error code.
        template<class FPtr>
        inline ResultCode GetFunction(const char* functionName, FPtr* pResult)
        {
            void* proc = GetFunctionImpl(functionName);

            if (proc)
            {
                *pResult = reinterpret_cast<FPtr>(proc);
                return ResultCode::Success;
            }

            return ResultCode::Fail;
        }

        inline static ResultCode Create(DynamicLibrary** ppLibrary)
        {
            *ppLibrary = AllocateObject<DynamicLibrary>();
            return ResultCode::Success;
        }
    };

    inline ResultCode DynamicLibrary::Init(std::string_view name)
    {
        std::string fullName(name);
        fullName += UN_DLL_EXTENSION;
        m_NativeHandle = UN_LoadLibrary(fullName.c_str());

        UN_Error(m_NativeHandle, "Library '{}{}' was not loaded due to an error", name, UN_DLL_EXTENSION);
        return m_NativeHandle ? ResultCode::Success : ResultCode::Fail;
    }

    inline void* DynamicLibrary::GetFunctionImpl(const char* functionName)
    {
        return UN_GetFunctionAddress(m_NativeHandle, functionName);
    }

    inline DynamicLibrary::~DynamicLibrary()
    {
        Unload();
    }

    inline void DynamicLibrary::Unload()
    {
        if (m_NativeHandle == nullptr)
        {
            return;
        }

        UN_FreeLibrary(m_NativeHandle);
        m_NativeHandle = nullptr;
    }
} // namespace UN
