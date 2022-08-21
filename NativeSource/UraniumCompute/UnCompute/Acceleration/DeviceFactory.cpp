#include <UnCompute/VulkanBackend/VulkanDeviceFactory.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode CreateDeviceFactory(BackendKind backendKind, IDeviceFactory** ppDeviceFactory)
        {
            static_assert(std::is_same_v<decltype(&CreateDeviceFactory), CreateDeviceFactoryProc>);

            InitializeLogger();
            switch (backendKind)
            {
            case BackendKind::Cpu:
                *ppDeviceFactory = nullptr;
                return ResultCode::NotImplemented;
            case BackendKind::Vulkan:
                {
                    VulkanDeviceFactory* pResult;
                    auto resultCode  = VulkanDeviceFactory::Create(&pResult);
                    *ppDeviceFactory = pResult;
                    return resultCode;
                }
            default:
                *ppDeviceFactory = nullptr;
                return ResultCode::InvalidArguments;
            }
        }
    }
} // namespace UN
