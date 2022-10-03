set(SRC
    Bindings/Acceleration/DeviceFactory.cpp
    Bindings/Backend/Buffer.cpp
    Bindings/Backend/CommandList.cpp
    Bindings/Backend/ComputeDevice.cpp
    Bindings/Backend/DeviceMemory.cpp
    Bindings/Backend/DeviceObject.cpp
    Bindings/Backend/Fence.cpp
    Bindings/Containers/HeapArray.cpp
    Bindings/Memory/Object.cpp

    UnCompute/Acceleration/AdapterInfo.h
    UnCompute/Acceleration/DeviceFactory.cpp
    UnCompute/Acceleration/IDeviceFactory.h

    UnCompute/Backend/BaseTypes.h
    UnCompute/Backend/BufferBase.cpp
    UnCompute/Backend/BufferBase.h
    UnCompute/Backend/CommandListBase.cpp
    UnCompute/Backend/CommandListBase.h
    UnCompute/Backend/DeviceMemoryBase.cpp
    UnCompute/Backend/DeviceMemoryBase.h
    UnCompute/Backend/DeviceObjectBase.h
    UnCompute/Backend/FenceBase.cpp
    UnCompute/Backend/FenceBase.h
    UnCompute/Backend/IBuffer.h
    UnCompute/Backend/ICommandList.h
    UnCompute/Backend/IComputeDevice.h
    UnCompute/Backend/IDeviceMemory.h
    UnCompute/Backend/IDeviceObject.h
    UnCompute/Backend/IFence.h
    UnCompute/Backend/IKernel.h
    UnCompute/Backend/IResourceBinding.h
    UnCompute/Backend/KernelBase.cpp
    UnCompute/Backend/KernelBase.h
    UnCompute/Backend/MemoryKindFlags.h
    UnCompute/Backend/ResourceBindingBase.cpp
    UnCompute/Backend/ResourceBindingBase.h

    UnCompute/Base/Base.h
    UnCompute/Base/Byte.h
    UnCompute/Base/CompilerTraits.h
    UnCompute/Base/Flags.h
    UnCompute/Base/Logger.h
    UnCompute/Base/Platform.h
    UnCompute/Base/PlatformInclude.h
    UnCompute/Base/PlatformTraits.h
    UnCompute/Base/ResultCode.h

    UnCompute/Compilation/IKernelCompiler.h
    UnCompute/Compilation/KernelCompiler.cpp
    UnCompute/Compilation/KernelCompiler.h

    UnCompute/Containers/ArraySlice.h
    UnCompute/Containers/HeapArray.h

    UnCompute/Memory/IAllocator.h
    UnCompute/Memory/Memory.h
    UnCompute/Memory/Object.h
    UnCompute/Memory/Ptr.h
    UnCompute/Memory/ReferenceCounter.h
    UnCompute/Memory/SystemAllocator.h

    UnCompute/Utils/DynamicLibrary.h
    UnCompute/Utils/MemoryUtils.h

    UnCompute/VulkanBackend/VulkanBuffer.cpp
    UnCompute/VulkanBackend/VulkanBuffer.h
    UnCompute/VulkanBackend/VulkanCommandList.cpp
    UnCompute/VulkanBackend/VulkanCommandList.h
    UnCompute/VulkanBackend/VulkanComputeDevice.cpp
    UnCompute/VulkanBackend/VulkanComputeDevice.h
    UnCompute/VulkanBackend/VulkanDescriptorAllocator.cpp
    UnCompute/VulkanBackend/VulkanDescriptorAllocator.h
    UnCompute/VulkanBackend/VulkanDeviceFactory.cpp
    UnCompute/VulkanBackend/VulkanDeviceFactory.h
    UnCompute/VulkanBackend/VulkanDeviceMemory.cpp
    UnCompute/VulkanBackend/VulkanDeviceMemory.h
    UnCompute/VulkanBackend/VulkanFence.cpp
    UnCompute/VulkanBackend/VulkanFence.h
    UnCompute/VulkanBackend/VulkanInclude.h
    UnCompute/VulkanBackend/VulkanKernel.cpp
    UnCompute/VulkanBackend/VulkanKernel.h
    UnCompute/VulkanBackend/VulkanResourceBinding.cpp
    UnCompute/VulkanBackend/VulkanResourceBinding.h
)

add_library(UnCompute SHARED ${SRC})

un_configure_target(UnCompute)
un_enable_sse_for_target(UnCompute)

target_include_directories(UnCompute PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}")

set_target_properties(UnCompute PROPERTIES FOLDER "UraniumCompute")
target_link_libraries(UnCompute volk spdlog dxc)

get_property("TARGET_SOURCE_FILES" TARGET UnCompute PROPERTY SOURCES)
source_group(TREE "${CMAKE_CURRENT_LIST_DIR}" FILES ${TARGET_SOURCE_FILES})

add_subdirectory(Tests)
