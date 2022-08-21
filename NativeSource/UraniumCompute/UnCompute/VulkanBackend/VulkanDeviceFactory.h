#pragma once
#include <UnCompute/Acceleration/AdapterInfo.h>
#include <UnCompute/Acceleration/IDeviceFactory.h>
#include <UnCompute/Backend/IDeviceObject.h>
#include <UnCompute/Containers/ArraySlice.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>
#include <vector>

namespace UN
{
    //! \brief This class holds a Vulkan API instance.
    class VulkanDeviceFactory final : public Object<IDeviceFactory>
    {
        VkInstance m_Instance;
        VkDebugReportCallbackEXT m_Debug;
        std::vector<VkPhysicalDevice> m_PhysicalDevices;
        std::vector<VkPhysicalDeviceProperties> m_PhysicalDeviceProperties;

    public:
        ~VulkanDeviceFactory() override;

        //! \brief Create a VkInstance, VkDebugReportCallbackEXT (if needed) and get physical device properties.
        ResultCode Init(const DeviceFactoryDesc& desc) override;
        void Reset() override;

        std::vector<AdapterInfo> EnumerateAdapters() override;

        [[nodiscard]] inline ArraySlice<const VkPhysicalDevice> GetVulkanAdapters() const
        {
            return m_PhysicalDevices;
        }

        [[nodiscard]] inline ArraySlice<const VkPhysicalDeviceProperties> GetVulkanAdapterProperties() const
        {
            return m_PhysicalDeviceProperties;
        }

        [[nodiscard]] BackendKind GetBackendKind() const override;
        ResultCode CreateDevice(IComputeDevice** ppDevice) override;

        static ResultCode Create(VulkanDeviceFactory** ppInstance);
    };
} // namespace UN
