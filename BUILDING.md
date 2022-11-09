# Building
To build and run the projects you will need:
- CMake *v3.17*
- C++ compiler with C++17 support
- VulkanSDK *v1.3.x*

Optional:
- .Net 6.0 (to build C# bindings)

## CMake Projects

To generate the native project files run CMake in the `NativeSource` directory.
```sh
cd UraniumCompute/NativeSource
cmake -B ./BuildDebug   -S . -DCMAKE_BUILD_TYPE=Debug   # if you need a debug build
cmake -B ./BuildRelease -S . -DCMAKE_BUILD_TYPE=Release # if you need a release build
```

*Note that build directory names **must** be `BuildDebug` and `BuildRelease`, otherwise the built libraries won't be found by .Net projects.*

## .Net Projects
First make sure that you have built the CMake projects.
When done, you can open the solution in `ManagedSource` directory in your IDE and build the library.

## Samples
We have sample projects in both C++ and C#.
They will help to get you started with UraniumCompute.
