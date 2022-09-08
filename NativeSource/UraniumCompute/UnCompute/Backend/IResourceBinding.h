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
        RWTexture       //!< Storage image with unordered access.
    };

    struct KernelResourceDesc
    {
        Int32 BindingIndex      = -1;                         //!< Binding index in the compute shader source.
        KernelResourceKind Kind = KernelResourceKind::Buffer; //!< Kind of resource that is bound to a kernel.
    };

    //! \brief Resource binding descriptor.
    struct ResourceBindingDesc
    {
        const char* Name = nullptr;                  //!< Resource binding debug name.
        ArraySlice<const KernelResourceDesc> Layout; //!< Array of kernel resource descriptors.
    };

    //! \brief Resource binding object used to bind resources to a compute kernel.
    class IResourceBinding : public IDeviceObject
    {
    public:
        using DescriptorType = ResourceBindingDesc;

        [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;

        virtual ResultCode Init(const DescriptorType& desc) = 0;
    };
} // namespace UN
