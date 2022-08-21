#pragma once
#include <UnCompute/Memory/IAllocator.h>

namespace UN
{
    //! \brief This allocator uses platform-specific aligned versions of malloc() and free().
    class SystemAllocator : public IAllocator
    {
        static SystemAllocator m_Instance;

    public:
        void* Allocate(USize size, USize alignment) override;
        void Deallocate(void* pointer) override;
        [[nodiscard]] const char* GetName() const override;

        //! \brief Get global static instance of the system allocator.
        inline static SystemAllocator* Get()
        {
            return &m_Instance;
        }
    };

    inline SystemAllocator SystemAllocator::m_Instance;

    inline void* SystemAllocator::Allocate(USize size, USize alignment)
    {
        return UN_ALIGNED_MALLOC(size, alignment);
    }

    inline void SystemAllocator::Deallocate(void* pointer)
    {
        return UN_ALIGNED_FREE(pointer);
    }

    inline const char* SystemAllocator::GetName() const
    {
        return "System allocator";
    }
} // namespace UN
