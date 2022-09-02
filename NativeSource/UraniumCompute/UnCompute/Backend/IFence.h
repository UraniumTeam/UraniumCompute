#pragma once
#include <UnCompute/Backend/IDeviceObject.h>

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
    };

    //! \brief An interface for fences - synchronization primitives that can be either signaled or reset.
    class IFence : public IDeviceObject
    {
    public:
        using DescriptorType = FenceDesc;

        [[nodiscard]] virtual const DescriptorType& GetDesc() const = 0;

        virtual ResultCode Init(const DescriptorType& desc) = 0;

        //! \brief Signal the fence on CPU.
        virtual void SignalOnCpu() = 0;

        //! \brief Wait for the fence to signal.
        virtual void WaitOnCpu() = 0;

        //! \brief Reset fence state.
        virtual void ResetState() = 0;

        //! \brief Get current fence state.
        virtual FenceState GetState() = 0;
    };
} // namespace UN
