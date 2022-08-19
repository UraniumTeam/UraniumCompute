#pragma once
#include <UnCompute/Memory/Object.h>
#include <UnCompute/Acceleration/AdapterInfo.h>
#include <vector>

namespace UN
{
    //! \brief Kind of backend, either a kind of GPU API or CPU.
    enum class BackendKind
    {
        Cpu,   //!< CPU backend, jobs on this kind of backend will be executed on the CPU.
        Vulkan //!< Vulkan backend, jobs on this kind of backend will be executed on the GPU
               //!< using Vulkan API compute shaders.
    };

    class IComputeDevice;
    struct ComputeDeviceDesc;

    //! \brief This class is used to create backend-specific compute devices.
    class IDeviceFactory : public IObject
    {
    public:
        //! \brief Initialize the compute device factory for a specific backend kind.
        //!
        //! \param backendKind - Kind of backend for the created devices.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode Init(BackendKind backendKind) = 0;

        //! \brief Get kind of backend for the compute devices created by this factory.
        [[nodiscard]] virtual BackendKind GetBackendKind() const = 0;

        //! \brief Get all adapters supported by the specified backend.
        [[nodiscard]] virtual std::vector<AdapterInfo> EnumerateAdapters() = 0;

        //! \brief Create a compute device.
        //!
        //! \param ppDevice - A pointer to memory where the pointer to the created device will be written.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode CreateDevice(IComputeDevice** ppDevice) = 0;
    };
} // namespace UN
