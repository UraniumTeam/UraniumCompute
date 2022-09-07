#pragma once
#include <UnCompute/Compilation/IKernelCompiler.h>
#include <UnCompute/Memory/Ptr.h>

namespace UN
{
    class DynamicLibrary;

    class KernelCompiler final : public Object<IKernelCompiler>
    {
        Ptr<DynamicLibrary> m_DynamicLibrary;
        DescriptorType m_Desc;

    public:
        ~KernelCompiler() override = default;

    private:
        [[nodiscard]] const DescriptorType& GetDesc() const override;
        ResultCode Init(const DescriptorType& desc) override;
        ResultCode Compile(const KernelCompilerArgs& args, HeapArray<Byte>* pResult) override;
    };
} // namespace UN
