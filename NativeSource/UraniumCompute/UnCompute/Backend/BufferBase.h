#pragma once
#include <UnCompute/Backend/DeviceObjectBase.h>
#include <UnCompute/Backend/IBuffer.h>

namespace UN
{
    class BufferBase : public DeviceObjectBase<IBuffer>
    {
    protected:
        virtual ResultCode InitInternal(const BufferDesc& desc) = 0;

        inline explicit BufferBase(IComputeDevice* pDevice)
            : DeviceObjectBase(pDevice)
        {
        }

    public:
        ResultCode Init(const BufferDesc& desc) override;
    };
} // namespace UN
