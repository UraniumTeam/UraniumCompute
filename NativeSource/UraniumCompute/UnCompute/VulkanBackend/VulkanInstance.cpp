#include <UnCompute/Memory/Memory.h>
#include <UnCompute/VulkanBackend/VulkanInstance.h>
#include <algorithm>
#include <iostream>

static VKAPI_ATTR VkBool32 VKAPI_CALL DebugReportCallback(VkDebugReportFlagsEXT /* flags */,
                                                          VkDebugReportObjectTypeEXT /* objectType */, UN::UInt64 /* object */,
                                                          size_t /* location */, UN::Int32 /* messageCode */,
                                                          const char* /* pLayerPrefix */, const char* pMessage,
                                                          void* /* pUserData */)
{
    constexpr static auto ignoredMessages = std::array{ "" };

    std::string_view message = pMessage;
    for (auto& msg : ignoredMessages)
    {
        if (message.find(msg) != std::string_view::npos)
        {
            return VK_FALSE;
        }
    }

    std::cout << "[Vulkan validation]: " << message << std::endl;
    return VK_FALSE;
}

namespace UN
{
    ResultCode VulkanInstance::Init(const std::string& applicationName)
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
                UN_Assert(found, "Vulkan instance extension not found");
                return ResultCode::Fail;
            }
        }

        VkApplicationInfo appInfo{};
        appInfo.sType            = VK_STRUCTURE_TYPE_APPLICATION_INFO;
        appInfo.apiVersion       = VK_API_VERSION_1_2;
        appInfo.pEngineName      = "UraniumCompute";
        appInfo.pApplicationName = applicationName.c_str();

        VkInstanceCreateInfo instanceCI{};
        instanceCI.sType                   = VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO;
        instanceCI.pApplicationInfo        = &appInfo;
        instanceCI.enabledLayerCount       = static_cast<UInt32>(RequiredInstanceLayers.size());
        instanceCI.ppEnabledLayerNames     = RequiredInstanceLayers.data();
        instanceCI.enabledExtensionCount   = static_cast<UInt32>(RequiredInstanceExtensions.size());
        instanceCI.ppEnabledExtensionNames = RequiredInstanceExtensions.data();

        vkCreateInstance(&instanceCI, VK_NULL_HANDLE, &m_Instance);
        volkLoadInstance(m_Instance);
#if UN_DEBUG
        VkDebugReportCallbackCreateInfoEXT debugCI{};
        debugCI.sType = VK_STRUCTURE_TYPE_DEBUG_REPORT_CALLBACK_CREATE_INFO_EXT;
        debugCI.flags |= VK_DEBUG_REPORT_WARNING_BIT_EXT;
        debugCI.flags |= VK_DEBUG_REPORT_PERFORMANCE_WARNING_BIT_EXT;
        debugCI.flags |= VK_DEBUG_REPORT_ERROR_BIT_EXT;
        debugCI.flags |= VK_DEBUG_REPORT_DEBUG_BIT_EXT;
        debugCI.pfnCallback = &DebugReportCallback;
        vkCreateDebugReportCallbackEXT(m_Instance, &debugCI, VK_NULL_HANDLE, &m_Debug);
#endif
        std::cout << "Vulkan instance created successfully" << std::endl;

        UInt32 adapterCount;
        vkEnumeratePhysicalDevices(m_Instance, &adapterCount, nullptr);
        m_PhysicalDevices.resize(adapterCount, VK_NULL_HANDLE);
        m_PhysicalDeviceProperties.reserve(adapterCount);
        vkEnumeratePhysicalDevices(m_Instance, &adapterCount, m_PhysicalDevices.data());
        for (auto& vkAdapter : m_PhysicalDevices)
        {
            auto& props = m_PhysicalDeviceProperties.emplace_back();
            vkGetPhysicalDeviceProperties(vkAdapter, &props);
        }

        return ResultCode::Success;
    }

    void VulkanInstance::Reset()
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
    }

    VulkanInstance::~VulkanInstance()
    {
        Reset();
    }

    std::vector<AdapterInfo> VulkanInstance::EnumerateAdapters()
    {
        std::vector<AdapterInfo> result;
        result.reserve(m_PhysicalDevices.size());
        for (USize i = 0; i < m_PhysicalDevices.size(); ++i)
        {
            auto& adapter = result.emplace_back();
            adapter.Id    = static_cast<Int>(i);

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

        return result;
    }

    ResultCode VulkanInstance::Create(VulkanInstance** ppInstance)
    {
        *ppInstance = AllocateObject<VulkanInstance>();
        (*ppInstance)->AddRef();
        return ResultCode::Success;
    }
} // namespace UN
