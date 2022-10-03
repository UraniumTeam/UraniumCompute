add_executable(ArrayTransformSample main.cpp)

un_configure_target(ArrayTransformSample)

set_target_properties(ArrayTransformSample PROPERTIES FOLDER "Samples")
target_link_libraries(ArrayTransformSample UnCompute)
