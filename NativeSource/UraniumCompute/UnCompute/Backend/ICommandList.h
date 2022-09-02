#pragma once
#include <UnCompute/Backend/IDeviceObject.h>
#include <UnCompute/Base/Flags.h>
#include <UnCompute/Memory/Ptr.h>

namespace UN
{
    //! \brief State of a command list.
    enum class CommandListState
    {
        Initial,
        Recording,
        Executable,
        Pending,
        Invalid
    };

    //! \brief Command list allocation flags.
    enum class CommandListFlags
    {
        None          = 0,
        OneTimeSubmit = UN_BIT(1) //!< The command list will be invalid after the first call to submit.
    };

    UN_ENUM_OPERATORS(CommandListFlags);

    //! \brief Command list descriptor.
    struct CommandListDesc
    {
        const char* Name       = nullptr;                //!< Command list debug name.
        CommandListFlags Flags = CommandListFlags::None; //!< Command list flags.

        inline CommandListDesc() = default;

        inline CommandListDesc(const char* name, CommandListFlags flags)
            : Name(name)
            , Flags(flags)
        {
        }
    };

    //! \brief Region for buffer copy command.
    struct BufferCopyRegion
    {
        UInt64 Size;         //!< Size of the copy region.
        UInt32 SourceOffset; //!< Offset in the source buffer.
        UInt32 DestOffset;   //!< Offset in the destination buffer.

        inline BufferCopyRegion() = default;

        inline explicit BufferCopyRegion(UInt64 size)
            : SourceOffset(0)
            , DestOffset(0)
            , Size(size)
        {
        }

        inline BufferCopyRegion(UInt32 sourceOffset, UInt32 destOffset, UInt64 size)
            : SourceOffset(sourceOffset)
            , DestOffset(destOffset)
            , Size(size)
        {
        }
    };

    class IFence;
    class ICommandList;
    class IBuffer;

    //! \brief Command list builder, used for device command recording.
    class CommandListBuilder
    {
        Ptr<ICommandList> m_pCommandList;

    public:
        explicit CommandListBuilder(ICommandList* pCommandList);

        //! \brief Set the command list state to CommandListState::Executable.
        void End();

        //! \brief Copy
        void Copy(IBuffer* pSource, IBuffer* pDestination, const BufferCopyRegion& region);
    };

    //! \brief An interface for command lists that record commands to be executed by the backend.
    class ICommandList : public IDeviceObject
    {
        friend class CommandListBuilder;

    protected:
        virtual void End() = 0;

        virtual void CmdCopy(IBuffer* pSource, IBuffer* pDestination, const BufferCopyRegion& region) = 0;

    public:
        using DescriptorType = CommandListDesc;

        [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;

        virtual ResultCode Init(const DescriptorType& desc) = 0;

        //! \brief Get the fence that is signaled after submit operation is complete.
        virtual IFence* GetFence() = 0;

        //! \brief Get command list state.
        [[nodiscard]] virtual CommandListState GetState() = 0;

        //! \brief Set the command list state to CommandListState::Recording.
        virtual CommandListBuilder Begin() = 0;

        //! \brief Set the command list state to CommandListState::Initial.
        virtual void ResetState() = 0;

        //! \brief Submit the command list and set the state to CommandListState::Pending.
        virtual ResultCode Submit() = 0;
    };

    inline CommandListBuilder::CommandListBuilder(ICommandList* pCommandList)
        : m_pCommandList(pCommandList)
    {
        UN_Assert(m_pCommandList->GetState() == CommandListState::Initial, "The command list must be in initial state");
    }

    inline void CommandListBuilder::End()
    {
        m_pCommandList->End();
        m_pCommandList.Reset();
    }

    inline void CommandListBuilder::Copy(IBuffer* pSource, IBuffer* pDestination, const BufferCopyRegion& region)
    {
        m_pCommandList->CmdCopy(pSource, pDestination, region);
    }
} // namespace UN
