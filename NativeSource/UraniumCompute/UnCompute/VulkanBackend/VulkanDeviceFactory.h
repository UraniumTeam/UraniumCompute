#pragma once
#include <UnCompute/Acceleration/AdapterInfo.h>
#include <UnCompute/Acceleration/IDeviceFactory.h>
#include <UnCompute/Backend/IDeviceObject.h>
#include <UnCompute/Containers/ArraySlice.h>
#include <UnCompute/Containers/HeapArray.h>
#include <UnCompute/VulkanBackend/VulkanInclude.h>

namespace UN
{
    //! \brief This class holds a Vulkan API instance.
    class VulkanDeviceFactory final : public Object<IDeviceFactory>
    {
        VkInstance m_Instance;
        VkDebugReportCallbackEXT m_Debug;

        HeapArray<AdapterInfo> m_Adapters;
        HeapArray<VkPhysicalDevice> m_PhysicalDevices;
        HeapArray<VkPhysicalDeviceProperties> m_PhysicalDeviceProperties;

    public:
        ~VulkanDeviceFactory() override;

        //! \brief Create a VkInstance, VkDebugReportCallbackEXT (if needed) and get physical device properties.
        ResultCode Init(const DeviceFactoryDesc& desc) override;
        void Reset() override;

        ArraySlice<const AdapterInfo> EnumerateAdapters() override;

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
