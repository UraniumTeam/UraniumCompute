#pragma once
#include <UnCompute/Backend/IBuffer.h>
#include <UnCompute/Backend/IComputeDevice.h>
#include <UnCompute/Backend/IDeviceMemory.h>
#include <algorithm>
#include <numeric>

namespace UN::Utility
{
    inline ResultCode AllocateMemoryFor(const ArraySlice<IDeviceObject*>& objects, MemoryKindFlags flags,
                                        const char* memoryDebugName, IDeviceMemory** ppMemory)
    {
        auto* pDevice = objects[0]->GetDevice();
        if (auto result = pDevice->CreateMemory(ppMemory); Failed(result))
        {
            return result;
        }

        DeviceMemoryDesc desc(memoryDebugName, flags, 0, objects);
        return (*ppMemory)->Init(desc);
    }

    inline ResultCode AllocateMemoryFor(IDeviceObject* pObject, MemoryKindFlags flags, IDeviceMemory** ppMemory)
    {
        auto name = fmt::format("Memory for \"{}\"", pObject->GetDebugName());
        IDeviceObject* objects[] = { pObject };
        return AllocateMemoryFor(objects, flags, name.c_str(), ppMemory);
    }
} // namespace UN::Utility
