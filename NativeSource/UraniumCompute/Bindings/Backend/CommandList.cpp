#include <UnCompute/Backend/ICommandList.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode ICommandList_Init(ICommandList* self, const CommandListDesc& desc)
        {
            return self->Init(desc);
        }

        UN_DLL_EXPORT void ICommandList_GetDesc(ICommandList* self, CommandListDesc& desc)
        {
            desc = self->GetDesc();
        }

        UN_DLL_EXPORT IFence* ICommandList_GetFence(ICommandList* self)
        {
            return self->GetFence();
        }

        UN_DLL_EXPORT CommandListState ICommandList_GetState(ICommandList* self)
        {
            return self->GetState();
        }

        UN_DLL_EXPORT bool ICommandList_Begin(ICommandList* self, CommandListBuilder& builder)
        {
            builder = self->Begin();
            return static_cast<bool>(builder);
        }

        UN_DLL_EXPORT void ICommandList_ResetState(ICommandList* self)
        {
            self->ResetState();
        }

        UN_DLL_EXPORT ResultCode ICommandList_Submit(ICommandList* self)
        {
            return self->Submit();
        }

        UN_DLL_EXPORT void CommandListBuilder_End(CommandListBuilder* self)
        {
            self->End();
        }

        UN_DLL_EXPORT void CommandListBuilder_MemoryBarrier(CommandListBuilder* self, IBuffer* pBuffer,
                                                            const MemoryBarrierDesc& barrierDesc)
        {
            self->MemoryBarrier(pBuffer, barrierDesc);
        }

        UN_DLL_EXPORT void CommandListBuilder_Copy(CommandListBuilder* self, IBuffer* pSource, IBuffer* pDestination,
                                                   const BufferCopyRegion& region)
        {
            self->Copy(pSource, pDestination, region);
        }

        UN_DLL_EXPORT void CommandListBuilder_Dispatch(CommandListBuilder* self, IKernel* pKernel, Int32 x, Int32 y, Int32 z)
        {
            self->Dispatch(pKernel, x, y, z);
        }
    }
} // namespace UN
