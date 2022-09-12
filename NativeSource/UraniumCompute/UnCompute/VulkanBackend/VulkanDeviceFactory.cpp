#include <UnCompute/Compilation/KernelCompiler.h>
#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanComputeDevice.h>
#include <UnCompute/VulkanBackend/VulkanDeviceFactory.h>
#include <algorithm>
#include <iostream>

constexpr auto RequiredInstanceLayers = std::array{ "VK_LAYER_KHRONOS_validation",
                                                    /*"VK_LAYER_LUNARG_standard_validation"*/
};

[[maybe_unused]] static VKAPI_ATTR VkBool32 VKAPI_CALL DebugReportCallback(VkDebugReportFlagsEXT flags,
                                                                           VkDebugReportObjectTypeEXT /* objectType */,
                                                                           UN::UInt64 /* object */, size_t /* location */,
                                                                           UN::Int32 /* messageCode */, const char* pLayerPrefix,
                                                                           const char* pMessage, void* /* pUserData */)
{
    constexpr static auto ignoredMessages = std::array<const char*, 0>{};

    std::string_view message = pMessage;
    for (auto& msg : ignoredMessages)
    {
        if (message.find(msg) != std::string_view::npos)
        {
            return VK_FALSE;
        }
    }

    switch (flags)
    {
    case VK_DEBUG_REPORT_INFORMATION_BIT_EXT:
        UNLOG_Info("[{}]: {}", message, pLayerPrefix);
        break;
    case VK_DEBUG_REPORT_WARNING_BIT_EXT:
    case VK_DEBUG_REPORT_PERFORMANCE_WARNING_BIT_EXT:
        UNLOG_Warning("[{}]: {}", message, pLayerPrefix);
        break;
    case VK_DEBUG_REPORT_ERROR_BIT_EXT:
        UNLOG_Error("[{}]: {}", message, pLayerPrefix);
        break;
    case VK_DEBUG_REPORT_DEBUG_BIT_EXT:
        return VK_FALSE;
        // UNLOG_Debug("[{}]: {}", message, pLayerPrefix);
        // break;
    default:
        UNLOG_Warning("[{}]: {}", message, pLayerPrefix);
        break;
    }

    return VK_FALSE;
}

namespace UN
{
    ResultCode VulkanDeviceFactory::Init(const DeviceFactoryDesc& desc)
    {
        volkInitialize();
        UInt32 layerCount;
        vkEnumerateInstanceLayerProperties(&layerCount, nullptr);
        std::vector<VkLayerProperties> layers(layerCount, VkLayerProperties{});
        vkEnumerateInstanceLayerProperties(&layerCount, layers.data());
        for (auto& layer : RequiredInstanceLayers)
        {
            auto layerSlice = std::string_view(layer);
            bool found      = std::any_of(layers.begin(), layers.end(), [&](const VkLayerProperties& props) {
                return layerSlice == props.layerName;
            });
            if (!found)
            {
                UN_Assert(found, "Vulkan instance layer not found");
                return ResultCode::Fail;
            }
        }

        UInt32 extensionCount;
        vkEnumerateInstanceExtensionProperties(nullptr, &extensionCount, nullptr);
        std::vector<VkExtensionProperties> extensions(extensionCount, VkExtensionProperties{});
        vkEnumerateInstanceExtensionProperties(nullptr, &extensionCount, extensions.data());
        for (auto& ext : RequiredInstanceExtensions)
        {
            auto extSlice = std::string_view(ext);
            bool found    = std::any_of(extensions.begin(), extensions.end(), [&](const VkExtensionProperties& props) {
                return extSlice == props.extensionName;
            });
            if (!found)
            {
                UN_Assert(found, "Vulkan instance extension '{}' not found", ext);
                return ResultCode::Fail;
            }
        }

        VkApplicationInfo appInfo{};
        appInfo.sType            = VK_STRUCTURE_TYPE_APPLICATION_INFO;
        appInfo.apiVersion       = VK_API_VERSION_1_2;
        appInfo.pEngineName      = "UraniumCompute";
        appInfo.pApplicationName = desc.ApplicationName;

        VkInstanceCreateInfo instanceCI{};
        instanceCI.sType                   = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
        instanceCI.pApplicationInfo        = &appInfo;
        instanceCI.enabledLayerCount       = static_cast<UInt32>(RequiredInstanceLayers.size());
        instanceCI.ppEnabledLayerNames     = RequiredInstanceLayers.data();
        instanceCI.enabledExtensionCount   = static_cast<UInt32>(RequiredInstanceExtensions.size());
        instanceCI.ppEnabledExtensionNames = RequiredInstanceExtensions.data();

        if (auto vkResult = vkCreateInstance(&instanceCI, VK_NULL_HANDLE, &m_Instance); !Succeeded(vkResult))
        {
            UN_Error(false, "Couldn't create Vulkan instance, vkCreateInstance returned {}", vkResult);
            return VulkanConvert(vkResult);
        }

        volkLoadInstance(m_Instance);

#if UN_DEBUG
        VkDebugReportCallbackCreateInfoEXT debugCI{};
        debugCI.sType = VK_STRUCTURE_TYPE_DEBUG_REPORT_CALLBACK_CREATE_INFO_EXT;
        debugCI.flags |= VK_DEBUG_REPORT_WARNING_BIT_EXT;
        debugCI.flags |= VK_DEBUG_REPORT_PERFORMANCE_WARNING_BIT_EXT;
        debugCI.flags |= VK_DEBUG_REPORT_ERROR_BIT_EXT;
        debugCI.flags |= VK_DEBUG_REPORT_DEBUG_BIT_EXT;
        debugCI.pfnCallback = &DebugReportCallback;

        if (auto vkResult = vkCreateDebugReportCallbackEXT(m_Instance, &debugCI, VK_NULL_HANDLE, &m_Debug); !Succeeded(vkResult))
        {
            UN_Error(false, "Couldn't create Vulkan debug report callback, vkCreateDebugReportCallbackEXT returned {}", vkResult);
            return VulkanConvert(vkResult);
        }

#endif
        UNLOG_Info("Vulkan instance created successfully");

        UInt32 adapterCount;
        vkEnumeratePhysicalDevices(m_Instance, &adapterCount, nullptr);

        m_PhysicalDevices.Resize(adapterCount);
        m_PhysicalDeviceProperties.Resize(adapterCount);
        m_Adapters.Resize(adapterCount);

        vkEnumeratePhysicalDevices(m_Instance, &adapterCount, m_PhysicalDevices.Data());

        for (UInt32 i = 0; i < m_PhysicalDevices.Length(); ++i)
        {
            auto& props = m_PhysicalDeviceProperties[i];
            vkGetPhysicalDeviceProperties(m_PhysicalDevices[i], &props);

            UNLOG_Info("Found Vulkan compatible GPU: {}", props.deviceName);

            auto& adapter = m_Adapters[i];
            adapter.Id    = i;
            memcpy(adapter.Name, m_PhysicalDeviceProperties[i].deviceName, std::min(256u, VK_MAX_PHYSICAL_DEVICE_NAME_SIZE));

            switch (m_PhysicalDeviceProperties[i].deviceType)
            {
            case VK_PHYSICAL_DEVICE_TYPE_INTEGRATED_GPU:
                adapter.Kind = AdapterKind::Integrated;
                break;
            case VK_PHYSICAL_DEVICE_TYPE_DISCRETE_GPU:
                adapter.Kind = AdapterKind::Discrete;
                break;
            case VK_PHYSICAL_DEVICE_TYPE_VIRTUAL_GPU:
                adapter.Kind = AdapterKind::Virtual;
                break;
            case VK_PHYSICAL_DEVICE_TYPE_CPU:
                adapter.Kind = AdapterKind::Cpu;
                break;
            default:
                adapter.Kind = AdapterKind::None;
                break;
            }
        }

        return ResultCode::Success;
    }

    void VulkanDeviceFactory::Reset()
    {
        if (m_Debug)
        {
            vkDestroyDebugReportCallbackEXT(m_Instance, m_Debug, VK_NULL_HANDLE);
            m_Debug = VK_NULL_HANDLE;
        }
        if (m_Instance)
        {
            vkDestroyInstance(m_Instance, VK_NULL_HANDLE);
            m_Instance = VK_NULL_HANDLE;
        }

        UNLOG_Debug("Destroyed Vulkan instance");
    }

    VulkanDeviceFactory::~VulkanDeviceFactory()
    {
        Reset();
    }

    ArraySlice<const AdapterInfo> VulkanDeviceFactory::EnumerateAdapters()
    {
        return m_Adapters;
    }

    BackendKind VulkanDeviceFactory::GetBackendKind() const
    {
        return BackendKind::Vulkan;
    }

    ResultCode VulkanDeviceFactory::CreateDevice(IComputeDevice** ppDevice)
    {
        VulkanComputeDevice* pResult;
        auto resultCode = VulkanComputeDevice::Create(this, &pResult);
        *ppDevice       = pResult;
        return resultCode;
    }

    ResultCode VulkanDeviceFactory::Create(VulkanDeviceFactory** ppInstance)
    {
        *ppInstance = AllocateObject<VulkanDeviceFactory>();
        (*ppInstance)->AddRef();
        return ResultCode::Success;
    }

    ResultCode VulkanDeviceFactory::CreateKernelCompiler(IKernelCompiler** ppCompiler)
    {
        *ppCompiler = AllocateObject<KernelCompiler>();
        (*ppCompiler)->AddRef();
        return ResultCode::Success;
    }
} // namespace UN
