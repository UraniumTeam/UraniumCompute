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
        if (auto state = GetState(); state != CommandListState::Initial)
        {
            UN_Assert(false, "Command list must be in initial state before Begin() can be called, but was in {}", state);
            return CommandListBuilder(nullptr);
        }

        m_State = CommandListState::Recording;
        if (auto resultCode = BeginInternal(); !Succeeded(resultCode))
        {
            UN_Assert(false, "Couldn't begin the command list, result was {}", resultCode);
            return CommandListBuilder(nullptr);
        }

        return CommandListBuilder(this);
    }

    void CommandListBase::ResetState()
    {
        if (GetState() == CommandListState::Initial)
        {
            UN_Warning(false, "The command list was already in initial state");
            return;
        }

        m_State = CommandListState::Initial;
        ResetStateInternal();
    }

    ResultCode CommandListBase::Submit()
    {
        if (auto state = GetState(); state != CommandListState::Executable)
        {
            UN_Error(false, "Command list must be in executable state before Submit() can be called, but was in {}", state);
            return ResultCode::InvalidOperation;
        }

        m_State = CommandListState::Pending;
        return SubmitInternal();
    }

    void CommandListBase::End()
    {
        if (auto state = GetState(); state != CommandListState::Recording)
        {
            UN_Assert(false, "Command list must be in recording state before End() can be called, but was in {}", state);
        }

        m_State = CommandListState::Executable;
        if (auto resultCode = EndInternal(); !Succeeded(resultCode))
        {
            UN_Assert(false, "Couldn't end the command list, result was {}", resultCode);
        }
    }

    IFence* CommandListBase::GetFence()
    {
        return m_pFence.Get();
    }
} // namespace UN
