if (WIN32)
    set(VOLK_STATIC_DEFINES VK_USE_PLATFORM_WIN32_KHR)
elseif()
    set(VOLK_STATIC_DEFINES VK_USE_PLATFORM_XCB_KHR)
endif()

add_subdirectory("${UN_PROJECT_ROOT}/ThirdParty/volk")
set_target_properties(volk PROPERTIES FOLDER "ThirdParty")
