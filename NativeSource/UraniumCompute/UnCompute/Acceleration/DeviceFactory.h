#pragma once
#include <UnCompute/Acceleration/IDeviceFactory.h>
#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanInstance.h>

namespace UN
{
    //! \brief Implementation of IDeviceFactory.
    class DeviceFactory final : public Object<IDeviceFactory>
    {
        BackendKind m_BackendKind{};
        Ptr<VulkanInstance> m_pVulkanInstance;

    public:
        //! \brief Create a Device factory.
        //!
        //! \param ppFactory - A pointer to memory where the pointer to the created device factory will be written.
        //!
        //! \return ResultCode::Success or an error code.
        inline static ResultCode Create(DeviceFactory** ppFactory)
        {
            *ppFactory = AllocateObject<DeviceFactory>();
            return ResultCode::Success;
        }

        ResultCode Init(BackendKind backendKind) override;

        [[nodiscard]] BackendKind GetBackendKind() const override;
        std::vector<AdapterInfo> EnumerateAdapters() override;
        ResultCode CreateDevice(IComputeDevice** ppDevice) override;
    };
} // namespace UN
