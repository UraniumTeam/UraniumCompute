#pragma once
#include <UnCompute/Backend/DeviceObjectBase.h>
#include <UnCompute/Backend/IResourceBinding.h>

namespace UN
{
    class ResourceBindingBase : public DeviceObjectBase<IResourceBinding>
    {
    protected:
        virtual ResultCode InitInternal(const DescriptorType& desc) = 0;

        inline explicit ResourceBindingBase(IComputeDevice* pDevice)
            : DeviceObjectBase(pDevice)
        {
        }

    public:
        ResultCode Init(const DescriptorType& desc) override;
    };
} // namespace UN
