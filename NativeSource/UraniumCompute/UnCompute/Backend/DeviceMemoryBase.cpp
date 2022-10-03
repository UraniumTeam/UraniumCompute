#include <UnCompute/Backend/DeviceMemoryBase.h>
#include <algorithm>

namespace UN
{
    ResultCode DeviceMemoryBase::Init(const DescriptorType& desc)
    {
        DeviceObjectBase::Init(desc.Name, desc);

        auto pDevice        = GetDevice();
        auto allDevicesSame = std::all_of(desc.Objects.begin(), desc.Objects.end(), [pDevice](IDeviceObject* object) {
            return object->GetDevice() == pDevice;
        });

        if (!allDevicesSame)
        {
            UN_Error(false, "All objects in DeviceMemoryDesc must created from a single device");
            return ResultCode::InvalidArguments;
        }

        return InitInternal(desc);
    }
} // namespace UN
