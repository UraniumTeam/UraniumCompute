#pragma once
#include <UnCompute/Acceleration/AdapterInfo.h>
#include <UnCompute/Memory/Object.h>
#include <UnCompute/Utils/DynamicLibrary.h>
#include <UnCompute/Containers/ArraySlice.h>

namespace UN
{
    //! \brief Kind of backend, either a kind of GPU API or CPU.
    enum class BackendKind
    {
        Cpu,   //!< CPU backend, jobs on this kind of backend will be executed on the CPU.
        Vulkan //!< Vulkan backend, jobs on this kind of backend will be executed on the GPU
               //! using Vulkan API compute shaders.
    };

    //! \brief IDeviceFactory descriptor.
    struct DeviceFactoryDesc
    {
        const char* ApplicationName; //!< Name of the application.

        inline DeviceFactoryDesc(const char* applicationName)
            : ApplicationName(applicationName)
        {
        }
    };

    class IComputeDevice;
    struct ComputeDeviceDesc;

    //! \brief This class is used to create backend-specific compute devices and related objects.
    class IDeviceFactory : public IObject
    {
    public:
        //! \brief Initialize the compute device factory.
        //!
        //! \param desc - Device factory descriptor.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode Init(const DeviceFactoryDesc& desc) = 0;

        //! \brief Reset the compute device factory.
        virtual void Reset() = 0;

        //! \brief Get kind of backend for the compute devices created by this factory.
        [[nodiscard]] virtual BackendKind GetBackendKind() const = 0;

        //! \brief Get all adapters supported by the specified backend.
        [[nodiscard]] virtual ArraySlice<const AdapterInfo> EnumerateAdapters() = 0;

        //! \brief Create a compute device.
        //!
        //! \param ppDevice - A pointer to memory where the pointer to the created device will be written.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode CreateDevice(IComputeDevice** ppDevice) = 0;
    };

    inline constexpr const char* CreateDeviceFactoryProcName = "CreateDeviceFactory";
    inline constexpr const char* UraniumComputeDllName       = "UnCompute";

    extern "C"
    {
        typedef ResultCode (*CreateDeviceFactoryProc)(BackendKind backendKind, IDeviceFactory** ppDeviceFactory);
    }

    //! \brief Load the UraniumCompute dynamic library and get the function to create IDeviceFactory instance.
    //!
    //! \param pCreateDeviceFactoryProc - A pointer in memory where the function pointer will be written.
    //!
    //! \return ResultCode::Success or an error code.
    inline ResultCode LoadCreateDeviceFactoryProc(DynamicLibrary** ppLibrary, CreateDeviceFactoryProc* pCreateDeviceFactoryProc)
    {
        // TODO: this function requires us to add DynamicLibrary.h include here which requires to include windows.h
        // and it's not desired. We can possibly create another STATIC library in the future dedicated for
        // DynamicLibrary class to avoid including platform headers.

        auto resultCode = DynamicLibrary::Create(ppLibrary);
        if (!Succeeded(resultCode))
        {
            return ResultCode::Fail;
        }

        resultCode = (*ppLibrary)->Init(UraniumComputeDllName);
        if (!Succeeded(resultCode))
        {
            UN_Error(false, "Couldn't load {} library", UraniumComputeDllName);
            return ResultCode::Fail;
        }

        resultCode = (*ppLibrary)->GetFunction(CreateDeviceFactoryProcName, pCreateDeviceFactoryProc);
        if (!Succeeded(resultCode))
        {
            UN_Error(false,
                     "Couldn't get the entry point named \"{}\" in \"{}\" library",
                     CreateDeviceFactoryProcName,
                     UraniumComputeDllName);
            return ResultCode::Fail;
        }

        return resultCode;
    }
} // namespace UN
