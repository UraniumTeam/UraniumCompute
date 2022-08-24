#include <UnCompute/Memory/Object.h>

namespace UN
{
    extern "C"
    {
        UN_DLL_EXPORT UInt32 IObject_AddRef(IObject* self)
        {
            return self->AddRef();
        }

        UN_DLL_EXPORT UInt32 IObject_Release(IObject* self)
        {
            return self->Release();
        }
    }
} // namespace UN
