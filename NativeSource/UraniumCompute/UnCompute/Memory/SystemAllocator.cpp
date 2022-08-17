#include <UnCompute/Memory/SystemAllocator.h>

namespace UN
{
    void* SystemAllocator::Allocate(USize size, USize alignment)
    {
        return UN_ALIGNED_MALLOC(size, alignment);
    }

    void SystemAllocator::Deallocate(void* pointer)
    {
        return UN_ALIGNED_FREE(pointer);
    }

    const char* SystemAllocator::GetName() const
    {
        return "System allocator";
    }
} // namespace UN
