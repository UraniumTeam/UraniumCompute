add_executable(MemoryBarriersSample main.cpp)

un_configure_target(MemoryBarriersSample)

set_target_properties(MemoryBarriersSample PROPERTIES FOLDER "Samples")
target_link_libraries(MemoryBarriersSample UnCompute)
