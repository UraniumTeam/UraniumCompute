cmake_minimum_required(VERSION 3.17.0)

project(UraniumCompute VERSION 0.1.0
	LANGUAGES CXX C
)

enable_testing()
set(UN_PROJECT_ROOT "${CMAKE_CURRENT_SOURCE_DIR}")
list(PREPEND CMAKE_MODULE_PATH "${UN_PROJECT_ROOT}/cmake")

include(Config)
include(ThirdParty/gtest)
include(ThirdParty/vulkan)
include(ThirdParty/dxc)
include(ThirdParty/spdlog)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)

add_subdirectory(UraniumCompute)
add_subdirectory(Samples)
