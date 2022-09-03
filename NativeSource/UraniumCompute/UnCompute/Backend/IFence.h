#pragma once
#include <UnCompute/Backend/IDeviceObject.h>
#include <chrono>

namespace UN
{
    //! \brief State of fence (signaled or reset).
    enum class FenceState
    {
        Signaled, //!< The fence was signaled.
        Reset     //!< The fence was reset or wasn't signaled.
    };

    //! \brief Fence descriptor.
    struct FenceDesc
    {
        const char* Name        = nullptr;           //!< Fence debug name.
        FenceState InitialState = FenceState::Reset; //!< Initial fence state.

        inline FenceDesc() = default;

        inline FenceDesc(const char* name, FenceState initialState = FenceState::Reset)
            : Name(name)
            , InitialState(initialState)
        {
        }
    };

    //! \brief An interface for fences - synchronization primitives that can be either signaled or reset.
    class IFence : public IDeviceObject
    {
    public:
        using DescriptorType = FenceDesc;

        [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;

        virtual ResultCode Init(const DescriptorType& desc) = 0;

        //! \brief Signal the fence on CPU.
        virtual ResultCode SignalOnCpu() = 0;

        //! \brief Wait for the fence to signal.
        //!
        //! \param timeout - Waiting timeout in nanoseconds.
        //!
        //! \return ResultCode::Success, ResultCode::Timeout or an error code.
        virtual ResultCode WaitOnCpu(std::chrono::nanoseconds timeout) = 0;

        //! \brief Wait for the fence to signal.
        //!
        //! Same as WaitOnCpu(std::chrono::nanoseconds), but without a timeout.
        //!
        //! \return ResultCode::Success or an error code.
        virtual ResultCode WaitOnCpu() = 0;

        //! \brief Reset fence state.
        virtual void ResetState() = 0;

        //! \brief Get current fence state.
        virtual FenceState GetState() = 0;
    };
} // namespace UN
