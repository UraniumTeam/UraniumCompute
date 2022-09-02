#include <UnCompute/Backend/FenceBase.h>

namespace UN
{
    ResultCode FenceBase::Init(const FenceDesc& desc)
    {
        DeviceObjectBase::Init(desc.Name, desc);
        return InitInternal(desc);
    }
} // namespace UN
