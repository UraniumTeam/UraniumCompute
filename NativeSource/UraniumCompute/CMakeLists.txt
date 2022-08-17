set(SRC
    UnCompute/Acceleration/DeviceFactory.cpp
    UnCompute/Acceleration/DeviceFactory.h
    UnCompute/Base/Base.h
    UnCompute/Base/CompilerTraits.h
    UnCompute/Base/Platform.h
    UnCompute/Base/PlatformInclude.h
    UnCompute/Base/PlatformTraits.h
    UnCompute/Backend/IComputeDevice.h
    UnCompute/Backend/IDeviceObject.h
    UnCompute/Memory/Object.h
    UnCompute/Memory/ReferenceCounter.h
    UnCompute/Memory/IAllocator.h
    UnCompute/Memory/Ptr.h
    UnCompute/Memory/Memory.h
    UnCompute/Memory/SystemAllocator.cpp
    UnCompute/Memory/SystemAllocator.h
    UnCompute/Base/ResultCode.h
    UnCompute/Acceleration/IDeviceFactory.h
    UnCompute/VulkanBackend/VulkanComputeDevice.cpp
    UnCompute/VulkanBackend/VulkanComputeDevice.h
    UnCompute/VulkanBackend/VulkanInstance.cpp
    UnCompute/VulkanBackend/VulkanInstance.h
    UnCompute/VulkanBackend/VulkanInclude.h
    UnCompute/Acceleration/AdapterInfo.h
)

add_library(UnCompute STATIC ${SRC})

un_configure_target(UnCompute)
un_enable_sse_for_target(UnCompute)

target_include_directories(UnCompute PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}")

set_target_properties(UnCompute PROPERTIES FOLDER "UraniumCompute")
target_link_libraries(UnCompute volk)

get_property("TARGET_SOURCE_FILES" TARGET UnCompute PROPERTY SOURCES)
source_group(TREE "${CMAKE_CURRENT_LIST_DIR}" FILES ${TARGET_SOURCE_FILES})

# add_subdirectory(Tests)
# add_subdirectory(Bindings)
