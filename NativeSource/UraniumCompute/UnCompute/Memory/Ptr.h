#pragma once
#include <UnCompute/Memory/Object.h>

namespace UN
{
    //! \internal
    namespace Internal
    {
        template<class T>
        class PtrRef final
        {
            T* m_Ptr;

        public:
            using ObjectType = typename T::ObjectType;

            inline explicit PtrRef(T* ptr)
                : m_Ptr(ptr)
            {
            }

            inline operator ObjectType**() // NOLINT
            {
                return m_Ptr->ReleaseAndGetAddressOf();
            }

            inline ObjectType* operator*()
            {
                return m_Ptr->Get();
            }
        };
    } // namespace Internal

    //! \brief Shared smart pointer implementation that uses reference counting.
    //!
    //! See ReferenceCounter for more information.
    //!
    //! \tparam T - Type of object to hold.
    //!
    //! \note T _must_ inherit from IObject.
    //! \see ReferenceCounter
    template<class T>
    class Ptr final
    {
        T* m_pObject;

        template<class T1>
        friend class Ptr;

        inline void InternalAddRef() const
        {
            if (m_pObject)
            {
                m_pObject->AddStrongRef();
            }
        }

        inline UInt32 InternalRelease()
        {
            UInt32 result = 0;
            if (m_pObject)
            {
                result    = m_pObject->ReleaseStrongRef();
                m_pObject = nullptr;
            }

            return result;
        }

    public:
        using ObjectType = T;

        //! \brief Create a _null_ reference counted pointer.
        inline Ptr() noexcept
            : m_pObject(nullptr)
        {
        }

        //! \brief Create a pointer that points to the specified object.
        //!
        //! \param pObject - The pointer to object.
        //!
        //! \note It is valid to use this constructor on a raw object since the object itself stores
        //!       The pointer to ReferenceCounter instance.
        inline Ptr(T* pObject) noexcept // NOLINT
            : m_pObject(pObject)
        {
            InternalAddRef();
        }

        //! \brief Copy a pointer (adds a strong reference to underlying object).
        //!
        //! \param other - Pointer to copy.
        inline Ptr(const Ptr& other) noexcept
            : m_pObject(other.m_pObject)
        {
            InternalAddRef();
        }

        //! \brief Copy a pointer (adds a strong reference to underlying object).
        //!
        //! \param other - Pointer to copy.
        template<class T1>
        inline Ptr(const Ptr<T1>& other) noexcept // NOLINT
            : m_pObject(other.Get())
        {
            InternalAddRef();
        }

        //! \brief Move a pointer (doesn't add a strong reference to underlying object).
        //!
        //! \param other - Pointer to move.
        inline Ptr(Ptr&& other) noexcept
            : m_pObject(other.m_pObject)
        {
            other.m_pObject = nullptr;
        }

        //! \brief Swap raw pointers of two objects without incrementing and decrementing ref-counters.
        inline void Swap(Ptr& other)
        {
            auto* t         = other.m_pObject;
            other.m_pObject = m_pObject;
            m_pObject       = t;
        }

        //! \brief Copy a pointer (adds a strong reference to underlying object).
        //!
        //! \param other - Pointer to copy.
        inline Ptr& operator=(const Ptr& other)
        {
            Ptr(other).Swap(*this);
            return *this;
        }

        //! \brief Move a pointer (doesn't add a strong reference to underlying object).
        //!
        //! \param other - Pointer to move.
        inline Ptr& operator=(Ptr&& other) noexcept
        {
            Ptr(std::move(other)).Swap(*this);
            return *this;
        }

        //! \brief Set a pointer to _null_.
        inline void Reset()
        {
            Ptr{}.Swap(*this);
        }

        inline ~Ptr()
        {
            InternalRelease();
        }

        //! \brief Get pointer to the underlying pointer.
        inline T* const* GetAddressOf() const
        {
            return &m_pObject;
        }

        //! \brief Get pointer to the underlying pointer.
        inline T** GetAddressOf()
        {
            return &m_pObject;
        }

        //! \brief Release a strong reference and get pointer to the stored pointer.
        //!
        //! It is the same as using unary '&' operator.
        inline T** ReleaseAndGetAddressOf()
        {
            InternalRelease();
            return &m_pObject;
        }

        //! \brief Attach a pointer and do not add strong reference.
        inline void Attach(T* pObject)
        {
            InternalRelease();
            m_pObject = pObject;
        }

        //! \brief Forget object and don't free it automatically.
        //!
        //! Can be useful to send an object to managed program.
        inline T* Detach()
        {
            T* ptr    = m_pObject;
            m_pObject = nullptr;
            return ptr;
        }

        //! \brief Get underlying raw pointer.
        inline T* Get() const
        {
            return m_pObject;
        }

        inline Internal::PtrRef<Ptr<T>> operator&() // NOLINT
        {
            return Internal::PtrRef<Ptr<T>>(this);
        }

        inline T& operator*()
        {
            return *Get();
        }

        inline T* operator->()
        {
            return Get();
        }

        inline const T& operator*() const
        {
            return *Get();
        }

        inline const T* operator->() const
        {
            return Get();
        }

        inline explicit operator bool() const
        {
            return m_pObject;
        }
    };

    template<class T>
    inline bool operator==(const Ptr<T>& lhs, std::nullptr_t)
    {
        return lhs.Get() == nullptr;
    }

    template<class T>
    inline bool operator!=(const Ptr<T>& lhs, std::nullptr_t)
    {
        return !(lhs == nullptr);
    }

    template<class T>
    inline bool operator==(std::nullptr_t, const Ptr<T>& rhs)
    {
        return rhs.Get() == nullptr;
    }

    template<class T>
    inline bool operator!=(std::nullptr_t, const Ptr<T>& rhs)
    {
        return !(nullptr == rhs);
    }

    template<class T1, class T2>
    inline bool operator==(const Ptr<T1>& lhs, const Ptr<T2>& rhs)
    {
        return static_cast<IObject*>(lhs.Get()) == static_cast<IObject*>(rhs.Get());
    }

    template<class T1, class T2>
    inline bool operator!=(const Ptr<T1>& lhs, const Ptr<T2>& rhs)
    {
        return !(lhs == rhs);
    }
} // namespace UN
