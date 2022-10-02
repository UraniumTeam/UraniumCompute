#pragma once
#include <UnCompute/Backend/IDeviceObject.h>
#include <UnCompute/Base/Byte.h>
#include <UnCompute/Containers/ArraySlice.h>

namespace UN
{
    class IResourceBinding;

    //! \brief Kernel descriptor.
    struct KernelDesc
    {
        const char* Name                   = nullptr; //!< Kernel debug name.
        IResourceBinding* pResourceBinding = nullptr; //!< Resource binding object that binds resources for the kernel.
        ArraySlice<const Byte> Bytecode;              //!< Kernel program bytecode.

        inline KernelDesc() = default;

        inline KernelDesc(const char* name, IResourceBinding* pResourceBinding, const ArraySlice<const Byte>& bytecode)
            : Name(name)
            , pResourceBinding(pResourceBinding)
            , Bytecode(bytecode)
        {
        }
    };

    //! \brief An interface for compute kernel - a program running on the device.
    class IKernel : public IDeviceObject
    {
    public:
        using DescriptorType = KernelDesc;

        [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;

        //! \brief Creates and initializes a kernel object.
        //!
        //! \param desc - Kernel descriptor.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode Init(const DescriptorType& desc) = 0;
    };
} // namespace UN
