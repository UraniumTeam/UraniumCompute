#pragma once
#include <UnCompute/Base/Base.h>
#include <array>
#include <volk.h>

#define VK_FLAGS_NONE 0

namespace UN
{
    constexpr auto RequiredInstanceLayers = std::array{ "VK_LAYER_KHRONOS_validation", "VK_LAYER_LUNARG_standard_validation" };

    constexpr auto RequiredInstanceExtensions = std::array{ VK_EXT_DEBUG_REPORT_EXTENSION_NAME,
                                                            VK_EXT_DEBUG_UTILS_EXTENSION_NAME,
                                                            VK_KHR_GET_PHYSICAL_DEVICE_PROPERTIES_2_EXTENSION_NAME };
} // namespace UN
