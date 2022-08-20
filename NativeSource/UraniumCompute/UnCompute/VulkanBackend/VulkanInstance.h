#pragma once
#include <UnCompute/Acceleration/AdapterInfo.h>
#include <UnCompute/Backend/IDeviceObject.h>
#include <UnCompute/Containers/ArraySlice.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>
#include <vector>

namespace UN
{
    //! \brief This class holds a Vulkan API instance.
    class VulkanInstance final : public Object<IObject>
    {
        VkInstance m_Instance;
        VkDebugReportCallbackEXT m_Debug;
        std::vector<VkPhysicalDevice> m_PhysicalDevices;
        std::vector<VkPhysicalDeviceProperties> m_PhysicalDeviceProperties;

    public:
        ~VulkanInstance() override;

        //! \brief Create a VkInstance, VkDebugReportCallbackEXT (if needed) and get physical device properties.
        ResultCode Init(const std::string& applicationName);
        void Reset();

        //! \brief Convert Vulkan physical devices to a vector of AdapterInfo structs.
        std::vector<AdapterInfo> EnumerateAdapters();

        [[nodiscard]] inline ArraySlice<const VkPhysicalDevice> GetVulkanAdapters() const
        {
            return m_PhysicalDevices;
        }

        [[nodiscard]] inline ArraySlice<const VkPhysicalDeviceProperties> GetVulkanAdapterProperties() const
        {
            return m_PhysicalDeviceProperties;
        }

        static ResultCode Create(VulkanInstance** ppInstance);
    };
} // namespace UN
