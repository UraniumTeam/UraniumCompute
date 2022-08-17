#include <UnCompute/Acceleration/DeviceFactory.h>
#include <UnCompute/Memory/Memory.h>
#include <iostream>

using namespace UN;

class TestObject : public Object<IObject>
{
public:
    inline TestObject()
    {
        std::cout << "Created object" << std::endl;
    }

    inline ~TestObject() override
    {
        std::cout << "Destroyed object" << std::endl;
    }

    Int Data = 123;
};

ResultCode TryCreateObject(TestObject** ppObject)
{
    if (ppObject)
    {
        *ppObject = AllocateObject<TestObject>();
        return *ppObject ? ResultCode::Success : ResultCode::Fail;
    }

    return ResultCode::InvalidArguments;
}

void PointerTest()
{
    Ptr<TestObject> pObject;
    if (UN_SUCCEEDED(TryCreateObject(&pObject)))
    {
        std::cout << pObject->Data << std::endl;

        auto pObjectCopy = pObject;
        std::cout << pObjectCopy->Data << std::endl;
        pObjectCopy->Data = 321;

        std::cout << pObject->Data << std::endl;
        std::cout << pObjectCopy->Data << std::endl;
    }
    if (UN_SUCCEEDED(TryCreateObject(&pObject)))
    {
        std::cout << pObject->Data << std::endl;

        auto pObjectCopy = pObject;
        std::cout << pObjectCopy->Data << std::endl;
        pObjectCopy->Data = 321;

        std::cout << pObject->Data << std::endl;
        std::cout << pObjectCopy->Data << std::endl;
    }
}

int main()
{
    PointerTest();
    Ptr<DeviceFactory> pFactory;
    DeviceFactory::Create(&pFactory);
    pFactory->Init(BackendKind::Vulkan);

    auto adapters = pFactory->EnumerateAdapters();
    for (auto& adapter : adapters)
    {
        std::cout << "Vulkan adapter #" << adapter.Id << ":\n";
        std::cout << "  Name = " << adapter.Name << "\n";
        std::cout << "  Kind = " << static_cast<Int>(adapter.Kind) << "\n";
        std::cout << std::endl;
    }
}
