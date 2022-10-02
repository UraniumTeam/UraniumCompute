#pragma once
#include <UnCompute/Backend/DeviceObjectBase.h>
#include <UnCompute/Backend/IKernel.h>

namespace UN
{
    class KernelBase : public DeviceObjectBase<IKernel>
    {
    protected:
        virtual ResultCode InitInternal(const DescriptorType& desc) = 0;

        inline explicit KernelBase(IComputeDevice* pDevice)
            : DeviceObjectBase(pDevice)
        {
        }

    public:
        ResultCode Init(const DescriptorType& desc) override;
    };
} // namespace UN
