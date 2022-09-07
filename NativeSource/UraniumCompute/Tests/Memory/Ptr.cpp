#include <Tests/Common/Common.h>
#include <UnCompute/Memory/Memory.h>

using namespace UN;

#define EXPECT_REF_COUNT(obj, count) EXPECT_EQ((obj)->GetRefCounter()->GetStrongRefCount(), (count))

class TestObject : public Object<IObject>
{
    Int32 m_Value = 0;

public:
    inline TestObject()           = default;
    inline ~TestObject() override = default;

    inline static ResultCode Create(TestObject** ppTestObject)
    {
        *ppTestObject = AllocateObject<TestObject>();
        (*ppTestObject)->AddRef();
        return ResultCode::Success;
    }

    inline virtual ResultCode Init(Int32 value)
    {
        m_Value = value;
        return ResultCode::Success;
    }

    [[nodiscard]] inline Int32 GetValue() const
    {
        return m_Value;
    }

    inline void SetValue(Int32 value)
    {
        m_Value = value;
    }
};

class DerivedTestObject final : public TestObject
{
    Int32 m_Value = 0;

public:
    inline DerivedTestObject()           = default;
    inline ~DerivedTestObject() override = default;

    inline static ResultCode Create(DerivedTestObject** ppDerivedTestObject)
    {
        *ppDerivedTestObject = AllocateObject<DerivedTestObject>();
        (*ppDerivedTestObject)->AddRef();
        return ResultCode::Success;
    }

    inline ResultCode Init(Int32 value) override
    {
        TestObject::Init(value * 2);
        m_Value = value;
        return ResultCode::Success;
    }

    [[nodiscard]] inline Int32 GetDerivedValue() const
    {
        return m_Value;
    }

    inline void SetDerivedValue(Int32 value)
    {
        m_Value = value;
    }
};

TEST(Ptr, NullConstructors)
{
    Ptr<TestObject> p1;
    Ptr<TestObject> p2(nullptr);
    Ptr<TestObject> p3(p2);
    Ptr<TestObject> p4 = p3;
    Ptr<TestObject> p5 = std::move(p4);
    Ptr<TestObject> p6(std::move(p3));

    for (const auto& ptr : { p1, p2, p3, p4, p5, p6 })
    {
        EXPECT_TRUE(!ptr);
        EXPECT_EQ(ptr, nullptr);
    }
}

TEST(Ptr, Constructors)
{
    Ptr<TestObject> p1;
    EXPECT_SUCCEEDED(TestObject::Create(&p1));
    EXPECT_SUCCEEDED(p1->Init(123));
    EXPECT_REF_COUNT(p1, 1);

    {
        Ptr<TestObject> p2 = p1;
        EXPECT_REF_COUNT(p1, 2);
        Ptr<TestObject> p3(p1);
        EXPECT_REF_COUNT(p1, 3);
        Ptr<TestObject> p4 = p1.Get();
        EXPECT_REF_COUNT(p1, 4);
        Ptr<TestObject> p5(p1.Get());
        EXPECT_REF_COUNT(p1, 5);

        for (const auto& ptr : { p2, p3, p4, p5 })
        {
            EXPECT_EQ(ptr, p1);
            EXPECT_EQ(ptr, p1.Get());
            EXPECT_EQ(ptr.Get(), p1);
            EXPECT_EQ(ptr.Get(), p1.Get());

            EXPECT_EQ(ptr->GetValue(), 123);
            EXPECT_EQ(ptr->GetValue(), p1->GetValue());
        }

        p5->SetValue(124);

        EXPECT_REF_COUNT(p1, 5);
    }

    EXPECT_REF_COUNT(p1, 1);

    {
        TestObject* pObject = p1.Get();
        Ptr<TestObject> p2  = std::move(p1);
        Ptr<TestObject> p3(std::move(p2));

        EXPECT_TRUE(!p1);
        EXPECT_TRUE(!p2);

        EXPECT_EQ(pObject, p3.Get());
        EXPECT_REF_COUNT(p3, 1);

        p1 = std::move(p3);
    }

    EXPECT_REF_COUNT(p1, 1);
}

TEST(Ptr, DerivedConstructor)
{
    Ptr<DerivedTestObject> p1;
    EXPECT_SUCCEEDED(DerivedTestObject::Create(&p1));
    EXPECT_SUCCEEDED(p1->Init(123));

    Ptr<TestObject> p2 = p1;
    EXPECT_EQ(p2->GetValue(), 246);
    EXPECT_EQ(un_verify_cast<DerivedTestObject*>(p2.Get())->GetDerivedValue(), 123);
    EXPECT_EQ(p2.As<DerivedTestObject>()->GetDerivedValue(), 123);

    EXPECT_REF_COUNT(p1, 2);
    EXPECT_REF_COUNT(p2, 2);

    EXPECT_EQ(p1, p2);
    EXPECT_EQ(p1.Get(), p2);
    EXPECT_EQ(p1, p2.Get());
    EXPECT_EQ(p1.Get(), p2.Get());
}

TEST(Ptr, Attach)
{
    TestObject* pObject;
    EXPECT_SUCCEEDED(TestObject::Create(&pObject));

    Ptr<TestObject> p1;

    p1.Attach(nullptr);
    EXPECT_TRUE(!p1);

    p1.Attach(pObject);
    EXPECT_TRUE(p1);
    EXPECT_EQ(p1.Get(), pObject);

    EXPECT_REF_COUNT(pObject, 1);

    p1.Attach(nullptr);
    EXPECT_TRUE(!p1);
}

TEST(Ptr, AttachAllocatedObject)
{
    Ptr<TestObject> p1 = AllocateObject<TestObject>();
    EXPECT_SUCCEEDED(p1->Init(123));
    EXPECT_REF_COUNT(p1, 1);

    p1.Attach(AllocateObject<TestObject>());
    EXPECT_NE(p1->GetValue(), 123);
    EXPECT_REF_COUNT(p1, 0);
    p1->AddRef();
}

TEST(Ptr, DetachNull)
{
    Ptr<TestObject> p1;
    auto* pDetached = p1.Detach();
    EXPECT_TRUE(!p1);
    EXPECT_EQ(pDetached, nullptr);
}

TEST(Ptr, DetachNonNull)
{
    Ptr<TestObject> p1;
    EXPECT_SUCCEEDED(TestObject::Create(&p1));
    EXPECT_SUCCEEDED(p1->Init(123));
    EXPECT_EQ(p1->GetValue(), 123);
    EXPECT_REF_COUNT(p1, 1);

    auto* pDetached = p1.Detach();
    EXPECT_TRUE(!p1);
    EXPECT_REF_COUNT(pDetached, 1);
    EXPECT_EQ(pDetached->GetValue(), 123);

    p1.Attach(pDetached);
    EXPECT_EQ(p1->GetValue(), 123);
    EXPECT_REF_COUNT(p1, 1);
    EXPECT_REF_COUNT(pDetached, 1);
}

TEST(Ptr, GetAddressOf)
{
    Ptr<TestObject> p1;
    {
        TestObject** ppTestObject = p1.GetAddressOf();
        *ppTestObject             = AllocateObject<TestObject>();
        EXPECT_EQ(p1, *ppTestObject);
        EXPECT_REF_COUNT(p1, 0);
        (*ppTestObject)->AddRef();
    }
    {
        EXPECT_REF_COUNT(p1, 1);
        Ptr<TestObject> p2 = p1;
        EXPECT_REF_COUNT(p1, 2);

        TestObject** ppTestObject = p2.ReleaseAndGetAddressOf();
        *ppTestObject             = AllocateObject<TestObject>();
        (*ppTestObject)->AddRef();
        EXPECT_EQ(p2, *ppTestObject);
        EXPECT_REF_COUNT(p1, 1);
        EXPECT_REF_COUNT(p2, 1);
    }
    {
        EXPECT_REF_COUNT(p1, 1);
        Ptr<TestObject> p2 = p1;
        EXPECT_REF_COUNT(p1, 2);

        TestObject** ppTestObject = &p2; // same as p2.ReleaseAndGetAddressOf()
        *ppTestObject             = AllocateObject<TestObject>();
        (*ppTestObject)->AddRef();
        EXPECT_EQ(p2, *ppTestObject);
        EXPECT_REF_COUNT(p1, 1);
        EXPECT_REF_COUNT(p2, 1);
    }

    EXPECT_REF_COUNT(p1, 1);
}
