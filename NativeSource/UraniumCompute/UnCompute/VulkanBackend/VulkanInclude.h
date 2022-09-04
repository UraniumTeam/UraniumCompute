#pragma once
#include <UnCompute/Base/Logger.h>
#include <array>
#include <volk.h>

namespace UN
{
    enum
    {
        VK_FLAGS_NONE = 0
    };

    constexpr auto RequiredInstanceLayers = std::array{ "VK_LAYER_KHRONOS_validation", "VK_LAYER_LUNARG_standard_validation" };

    constexpr auto RequiredInstanceExtensions = std::array{ VK_EXT_DEBUG_REPORT_EXTENSION_NAME,
                                                            VK_EXT_DEBUG_UTILS_EXTENSION_NAME,
                                                            VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME };

    constexpr auto DescriptorTypeMaxValue = VK_DESCRIPTOR_TYPE_INPUT_ATTACHMENT + 1;

    //! \brief Check if Vulkan result succeeded.
    inline bool Succeeded(VkResult result)
    {
        return result == VK_SUCCESS;
    }

    inline const char* VulkanResultToString(VkResult result)
    {
        switch (result)
        {
            // clang-format off
        case VK_SUCCESS: return "VkResult::VK_SUCCESS";
        case VK_NOT_READY: return "VkResult::VK_NOT_READY";
        case VK_TIMEOUT: return "VkResult::VK_TIMEOUT";
        case VK_EVENT_SET: return "VkResult::VK_EVENT_SET";
        case VK_EVENT_RESET: return "VkResult::VK_EVENT_RESET";
        case VK_INCOMPLETE: return "VkResult::VK_INCOMPLETE";
        case VK_ERROR_OUT_OF_HOST_MEMORY: return "VkResult::VK_ERROR_OUT_OF_HOST_MEMORY";
        case VK_ERROR_OUT_OF_DEVICE_MEMORY: return "VkResult::VK_ERROR_OUT_OF_DEVICE_MEMORY";
        case VK_ERROR_INITIALIZATION_FAILED: return "VkResult::VK_ERROR_INITIALIZATION_FAILED";
        case VK_ERROR_DEVICE_LOST: return "VkResult::VK_ERROR_DEVICE_LOST";
        case VK_ERROR_MEMORY_MAP_FAILED: return "VkResult::VK_ERROR_MEMORY_MAP_FAILED";
        case VK_ERROR_LAYER_NOT_PRESENT: return "VkResult::VK_ERROR_LAYER_NOT_PRESENT";
        case VK_ERROR_EXTENSION_NOT_PRESENT: return "VkResult::VK_ERROR_EXTENSION_NOT_PRESENT";
        case VK_ERROR_FEATURE_NOT_PRESENT: return "VkResult::VK_ERROR_FEATURE_NOT_PRESENT";
        case VK_ERROR_INCOMPATIBLE_DRIVER: return "VkResult::VK_ERROR_INCOMPATIBLE_DRIVER";
        case VK_ERROR_TOO_MANY_OBJECTS: return "VkResult::VK_ERROR_TOO_MANY_OBJECTS";
        case VK_ERROR_FORMAT_NOT_SUPPORTED: return "VkResult::VK_ERROR_FORMAT_NOT_SUPPORTED";
        case VK_ERROR_FRAGMENTED_POOL: return "VkResult::VK_ERROR_FRAGMENTED_POOL";
        case VK_ERROR_UNKNOWN: return "VkResult::VK_ERROR_UNKNOWN";
        case VK_ERROR_OUT_OF_POOL_MEMORY: return "VkResult::VK_ERROR_OUT_OF_POOL_MEMORY";
        case VK_ERROR_INVALID_EXTERNAL_HANDLE: return "VkResult::VK_ERROR_INVALID_EXTERNAL_HANDLE";
        case VK_ERROR_FRAGMENTATION: return "VkResult::VK_ERROR_FRAGMENTATION";
        case VK_ERROR_INVALID_OPAQUE_CAPTURE_ADDRESS: return "VkResult::VK_ERROR_INVALID_OPAQUE_CAPTURE_ADDRESS";
        case VK_ERROR_VALIDATION_FAILED_EXT: return "VkResult::VK_ERROR_VALIDATION_FAILED_EXT";
            // clang-format on
        default:
            UN_Assert(false, "VkResult was unknown");
            return "VkResult::<UNKNOWN>";
        }
    }

    inline ResultCode VulkanConvert(VkResult result)
    {
        switch (result)
        {
        case VK_SUCCESS:
            return ResultCode::Success;
        case VK_TIMEOUT:
            return ResultCode::Timeout;
        case VK_ERROR_OUT_OF_HOST_MEMORY:
        case VK_ERROR_OUT_OF_DEVICE_MEMORY:
        case VK_ERROR_OUT_OF_POOL_MEMORY:
            return ResultCode::OutOfMemory;
        default:
            return ResultCode::Fail;
        }
    }
} // namespace UN

template<>
struct fmt::formatter<VkResult> : fmt::formatter<std::string_view>
{
    template<typename FormatContext>
    auto format(const VkResult& result, FormatContext& ctx) const -> decltype(ctx.out())
    {
        return fmt::format_to(ctx.out(), "{}", UN::VulkanResultToString(result));
    }
};
