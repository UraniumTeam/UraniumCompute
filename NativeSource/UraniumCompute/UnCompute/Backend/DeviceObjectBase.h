#pragma once
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Backend/IDeviceObject.h>
#include <UnCompute/Memory/Ptr.h>

namespace UN
{
    //! \brief Base class for all compute backend objects.
    //!
    //! TInterface must be derived from IDeviceObject and must have these members:
    //!
    //! \code{.cpp}
    //!     using DescriptorType = ...;
    //!     virtual const DescriptorType& GetDesc() const = 0;
    //! \endcode
    template<class TInterface, std::enable_if_t<std::is_base_of_v<IDeviceObject, TInterface>, bool> = true>
    class DeviceObjectBase : public Object<TInterface>
    {
    public:
        using DescriptorType = typename TInterface::DescriptorType;

    protected:
        Ptr<IComputeDevice> m_pDevice;
        DescriptorType m_Desc;

        //! \brief Common device object initializer.
        //!
        //! \param desc    - Object descriptor.
        //! \param pDevice - Compute device this object was created on.
        inline void Init(const DescriptorType& desc, IComputeDevice* pDevice)
        {
            m_pDevice = pDevice;
            m_Desc    = desc;
        }

    public:
        inline const DescriptorType& GetDesc() const override
        {
            return m_Desc;
        }

        [[nodiscard]] inline IComputeDevice* GetDevice() const override
        {
            return m_pDevice.Get();
        }
    };
} // namespace UN
