#pragma once
#include <UnCompute/Backend/DeviceObjectBase.h>
#include <UnCompute/Backend/IFence.h>

namespace UN
{
    class FenceBase : public DeviceObjectBase<IFence>
    {
    protected:
        virtual ResultCode InitInternal(const DescriptorType& desc) = 0;

        inline explicit FenceBase(IComputeDevice* pDevice)
            : DeviceObjectBase(pDevice)
        {
        }

    public:
        ResultCode Init(const DescriptorType& desc) override;
    };
} // namespace UN
