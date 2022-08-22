#include <UnCompute/Backend/DeviceMemoryBase.h>

namespace UN
{
    ResultCode DeviceMemoryBase::Init(const IDeviceMemory::DescriptorType& desc)
    {
        DeviceObjectBase<IDeviceMemory>::Init(desc.Name, desc);
        return InitInternal(desc);
    }
} // namespace UN
