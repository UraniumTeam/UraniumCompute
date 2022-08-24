#include <UnCompute/Containers/HeapArray.h>

namespace UN
{
    using Array = HeapArray<Int8>;
    using Slice = ArraySlice<Int8>;

    extern "C"
    {
        UN_DLL_EXPORT void HeapArray_CreateEmpty(Array* pArray)
        {
            new (pArray) Array();
        }

        UN_DLL_EXPORT void HeapArray_CreateWithSize(Array* pArray, UInt64 size)
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
}
