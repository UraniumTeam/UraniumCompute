#pragma once
#include <UnCompute/Base/Base.h>

namespace UN
{
    //! \brief An interface for memory allocators.
    class IAllocator
    {
    public:
        //! \brief Allocate memory.
        //!
        //! \param size - Size of the allocation in bytes.
        //! \param alignment - Alignment of the allocation in bytes.
        //!
        //! \return Pointer to the allocated block of memory.
        virtual void* Allocate(USize size, USize alignment) = 0;

        //! \brief Deallocate memory.
        //!
        //! \param pointer - A pointer to the block of memory to deallocate.
        virtual void Deallocate(void* pointer) = 0;

        //! \brief Get debug name of the allocator.
        [[nodiscard]] virtual const char* GetName() const = 0;
    };
} // namespace UN
