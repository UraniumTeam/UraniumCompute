#include <UnCompute/Backend/IResourceBinding.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT ResultCode IResourceBinding_Init(IResourceBinding* self, const ResourceBindingDesc& desc)
        {
            return self->Init(desc);
        }

        UN_DLL_EXPORT void IResourceBinding_GetDesc(IResourceBinding* self, ResourceBindingDesc& desc)
        {
            desc = self->GetDesc();
        }

        UN_DLL_EXPORT ResultCode IResourceBinding_SetVariable(IResourceBinding* self, Int32 bindingIndex, IBuffer* pBuffer)
        {
            return self->SetVariable(bindingIndex, pBuffer);
        }
    }
} // namespace UN
