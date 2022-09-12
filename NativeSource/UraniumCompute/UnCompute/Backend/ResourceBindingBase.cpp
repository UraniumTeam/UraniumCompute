#include <UnCompute/Backend/ResourceBindingBase.h>

namespace UN
{
    ResultCode ResourceBindingBase::Init(const DescriptorType& desc)
    {
        DeviceObjectBase::Init(desc.Name, desc);
        return InitInternal(desc);
    }
} // namespace UN
