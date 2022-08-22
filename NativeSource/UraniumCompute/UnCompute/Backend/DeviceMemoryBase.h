#pragma once
#include <UnCompute/Backend/DeviceObjectBase.h>
#include <UnCompute/Backend/IDeviceMemory.h>

namespace UN
{
    class DeviceMemoryBase : public DeviceObjectBase<IDeviceMemory>
    {
    protected:
        virtual ResultCode InitInternal(const DescriptorType& desc) = 0;

    public:
        ResultCode Init(const DescriptorType& desc) final;
    };
} // namespace UN
