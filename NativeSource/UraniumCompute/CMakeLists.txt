set(SRC
    UnCompute/Acceleration/DeviceFactory.cpp
    UnCompute/Acceleration/DeviceFactory.h

    UnCompute/Base/Base.h
    UnCompute/Base/CompilerTraits.h
    UnCompute/Base/Platform.h
    UnCompute/Base/PlatformInclude.h
    UnCompute/Base/PlatformTraits.h
)

add_library(UraniumCompute STATIC ${SRC})

target_include_directories(UraniumCompute PUBLIC "${CMAKE_CURRENT_SOURCE_DIR}")

set_target_properties(UraniumCompute PROPERTIES FOLDER "UraniumCompute")
target_link_libraries(UraniumCompute volk)

get_property("TARGET_SOURCE_FILES" TARGET UraniumCompute PROPERTY SOURCES)
source_group(TREE "${CMAKE_CURRENT_LIST_DIR}" FILES ${TARGET_SOURCE_FILES})

# add_subdirectory(Tests)
# add_subdirectory(Bindings)
