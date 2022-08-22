#include <UnCompute/Backend/BufferBase.h>

namespace UN
{
    ResultCode BufferBase::Init(const BufferDesc& desc)
    {
        DeviceObjectBase<IBuffer>::Init(desc.Name, desc);
        return InitInternal(desc);
    }
} // namespace UN
