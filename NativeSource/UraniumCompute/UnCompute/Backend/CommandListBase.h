#pragma once
#include <UnCompute/Backend/DeviceObjectBase.h>
#include <UnCompute/Backend/ICommandList.h>
#include <UnCompute/Backend/IFence.h>

namespace UN
{
    class CommandListBase : public DeviceObjectBase<ICommandList>
    {
    protected:
        CommandListState m_State = CommandListState::Invalid;
        Ptr<IFence> m_pFence;

        virtual ResultCode InitInternal(const CommandListDesc& desc) = 0;
        virtual ResultCode BeginInternal()                           = 0;
        virtual ResultCode EndInternal()                             = 0;
        virtual ResultCode ResetStateInternal()                      = 0;
        virtual ResultCode SubmitInternal()                          = 0;

        void End() override;

        inline explicit CommandListBase(IComputeDevice* pDevice)
            : DeviceObjectBase(pDevice)
        {
        }

    public:
        ResultCode Init(const CommandListDesc& desc) override;

        IFence* GetFence() override;

        CommandListBuilder Begin() override;
        void ResetState() override;
        ResultCode Submit() override;
    };
} // namespace UN
