#include <UnCompute/Backend/CommandListBase.h>

namespace UN
{
    ResultCode CommandListBase::Init(const CommandListDesc& desc)
    {
        m_State = CommandListState::Initial;
        DeviceObjectBase::Init(desc.Name, desc);
        return InitInternal(desc);
    }

    CommandListBuilder CommandListBase::Begin()
    {
        m_State = CommandListState::Recording;
        if (auto resultCode = BeginInternal(); !Succeeded(resultCode))
        {
            UN_Error(false, "Couldn't begin the command list, result was {}", resultCode);
        }

        return CommandListBuilder(this);
    }

    void CommandListBase::ResetState()
    {
        m_State = CommandListState::Initial;
        ResetStateInternal();
    }

    ResultCode CommandListBase::Submit()
    {
        m_State = CommandListState::Pending;
        return SubmitInternal();
    }

    void CommandListBase::End()
    {
        m_State = CommandListState::Executable;
        if (auto resultCode = EndInternal(); !Succeeded(resultCode))
        {
            UN_Error(false, "Couldn't end the command list, result was {}", resultCode);
        }
    }

    IFence* CommandListBase::GetFence()
    {
        return m_pFence.Get();
    }
} // namespace UN
