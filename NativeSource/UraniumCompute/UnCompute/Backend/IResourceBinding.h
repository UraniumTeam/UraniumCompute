#pragma once
#include <UnCompute/Backend/IDeviceMemory.h>

namespace UN
{
    //! \brief Kind of resource that is bound to a kernel.
    enum class KernelResourceKind
    {
        Buffer,         //!< Read-only buffer.
        ConstantBuffer, //!< Constant buffer.
        RWBuffer,       //!< Storage buffer with unordered access.
        SampledTexture, //!< Read-only sampled image.
        RWTexture,      //!< Storage image with unordered access.
        Sampler         //!< Texture sampler.
    };

    //! \brief Kernel resource descriptor.
    struct KernelResourceDesc
    {
        Int32 BindingIndex      = -1;                         //!< Binding index in the compute shader source.
        KernelResourceKind Kind = KernelResourceKind::Buffer; //!< Kind of resource that is bound to a kernel.

        inline KernelResourceDesc() = default;

        inline KernelResourceDesc(Int32 bindingIndex, KernelResourceKind kind)
            : BindingIndex(bindingIndex)
            , Kind(kind)
        {
        }
    };

    //! \brief Resource binding descriptor.
    struct ResourceBindingDesc
    {
        const char* Name = nullptr;                  //!< Resource binding debug name.
        ArraySlice<const KernelResourceDesc> Layout; //!< Array of kernel resource descriptors.

        inline ResourceBindingDesc() = default;

        inline ResourceBindingDesc(const char* name, const ArraySlice<const KernelResourceDesc>& layout)
            : Name(name)
            , Layout(layout)
        {
        }
    };

    class IBuffer;

    //! \brief Resource binding object used to bind resources to a compute kernel.
    class IResourceBinding : public IDeviceObject
    {
    public:
        using DescriptorType = ResourceBindingDesc;

        [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;

        //! \brief Set kernel variable.
        //!
        //! \param bindingIndex - Binding index of the variable to set.
        //! \param pBuffer      - The buffer to assign.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode SetVariable(Int32 bindingIndex, IBuffer* pBuffer) = 0;

        virtual ResultCode Init(const DescriptorType& desc) = 0;
    };
} // namespace UN
