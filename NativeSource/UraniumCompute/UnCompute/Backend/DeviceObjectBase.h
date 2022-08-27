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
    //!     [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;
    //! \endcode
    template<class TInterface, std::enable_if_t<std::is_base_of_v<IDeviceObject, TInterface>, bool> = true>
    class DeviceObjectBase : public Object<TInterface>
    {
    public:
        using DescriptorType = typename TInterface::DescriptorType;

    protected:
        Ptr<IComputeDevice> m_pDevice;
        DescriptorType m_Desc;
        std::string m_Name;

        //! \brief Common device object initializer.
        //!
        //! \param name - Object debug name.
        //! \param desc - Object descriptor.
        inline void Init(std::string_view name, const DescriptorType& desc)
        {
            m_Name = name;
            m_Desc = desc;
        }

        inline explicit DeviceObjectBase(IComputeDevice* pDevice)
            : m_pDevice(pDevice)
        {
        }

    public:
        inline const DescriptorType& GetDesc() const override
        {
            return m_Desc;
        }

        [[nodiscard]] inline std::string_view GetDebugName() const override
        {
            return m_Name;
        }

        [[nodiscard]] inline IComputeDevice* GetDevice() const override
        {
            return m_pDevice.Get();
        }

        [[nodiscard]] inline UInt32 Release() override
        {
            Ptr<IComputeDevice> pDevice;
            return Object<TInterface>::Release([this, &pDevice] {
                pDevice = m_pDevice;
            });
        }
    };
} // namespace UN
