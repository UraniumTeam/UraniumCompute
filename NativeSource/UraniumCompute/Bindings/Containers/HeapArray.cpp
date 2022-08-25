#include <UnCompute/Containers/HeapArray.h>

namespace UN
{
    using Array = HeapArray<Int8>;
    using Slice = ArraySlice<Int8>;

    extern "C"
    {
        UN_DLL_EXPORT void HeapArray_Destroy(Array* self)
        {
            self->Reset();
        }

        UN_DLL_EXPORT void HeapArray_CreateWithSize(UInt64 size, Array* pArray)
        {
            new (pArray) Array(size);
        }

        UN_DLL_EXPORT void HeapArray_Resize(Array* self, UInt64 size)
        {
            self->Resize(size);
        }

        UN_DLL_EXPORT UInt64 HeapArray_CopyDataTo(Array* self, Slice* arraySlice)
        {
            return self->CopyDataTo(*arraySlice);
        }
    }
} // namespace UN
