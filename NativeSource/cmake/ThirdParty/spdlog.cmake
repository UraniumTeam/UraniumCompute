add_subdirectory("${UN_PROJECT_ROOT}/ThirdParty/spdlog")
set_target_properties(spdlog PROPERTIES FOLDER "ThirdParty")

if (NOT UN_COMPILER_MSVC)
    target_compile_options(spdlog PRIVATE -fPIC)
endif ()
