#include <UnCompute/Backend/KernelBase.h>

namespace UN
{
    ResultCode KernelBase::Init(const DescriptorType& desc)
    {
        DeviceObjectBase::Init(desc.Name, desc);
        return InitInternal(desc);
    }
} // namespace UN
