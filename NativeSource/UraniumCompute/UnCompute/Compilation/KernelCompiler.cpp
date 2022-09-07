#include <UnCompute/Compilation/KernelCompiler.h>
#include <UnCompute/Utils/DynamicLibrary.h>

#include <d3d11shader.h>
#include <dxc/DxilContainer/DxilContainer.h>
#include <dxc/dxcapi.h>

namespace UN
{
    inline ResultCode ConvertResult(HRESULT hr)
    {
        if (SUCCEEDED(hr))
        {
            return ResultCode::Success;
        }

        switch (hr)
        {
        case E_FAIL:
            return ResultCode::Fail;
        case E_ABORT:
            return ResultCode::Abort;
        case E_NOTIMPL:
            return ResultCode::NotImplemented;
        case E_INVALIDARG:
            return ResultCode::InvalidArguments;
        case E_ACCESSDENIED:
            return ResultCode::AccessDenied;
        case E_OUTOFMEMORY:
            return ResultCode::OutOfMemory;
        default:
            return ResultCode::Fail;
        }
    }

    inline LPCWSTR GetTargetProfile(KernelSourceLang lang)
    {
        UN_Verify(lang == KernelSourceLang::Hlsl, "Kernel source language {} is not supported", static_cast<Int32>(lang));
        return L"cs_6_0";
    }

    inline LPCWSTR ConvertOptLevel(CompilerOptimizationLevel level)
    {
        switch (level)
        {
        case CompilerOptimizationLevel::None:
            return DXC_ARG_SKIP_OPTIMIZATIONS;
        case CompilerOptimizationLevel::O1:
            return DXC_ARG_OPTIMIZATION_LEVEL1;
        case CompilerOptimizationLevel::O2:
            return DXC_ARG_OPTIMIZATION_LEVEL2;
        case CompilerOptimizationLevel::O3:
            return DXC_ARG_OPTIMIZATION_LEVEL3;
        default:
            UN_Error(false, "Unknown CompilerOptimizationLevel::<{}>", static_cast<Int32>(level));
            return nullptr;
        }
    }

    class IncludeHandler : public IDxcIncludeHandler
    {
        std::atomic_int m_RefCounter;
        std::wstring m_BasePath;
        IDxcLibrary* m_Library;

    public:
        inline IncludeHandler(const std::wstring& basePath, IDxcLibrary* library)
            : m_BasePath(basePath)
            , m_Library(library)
            , m_RefCounter(0)
        {
        }

        inline HRESULT QueryInterface(const IID&, void**) override
        {
            return E_FAIL;
        }

        inline ULONG AddRef() override
        {
            return m_RefCounter++;
        }

        inline ULONG Release() override
        {
            return m_RefCounter--;
        }

        inline HRESULT LoadSource(LPCWSTR pFilename, IDxcBlob** ppIncludeSource) override
        {
            UN_Error(false, "HLSL Include handler is not implemented");
            auto path = m_BasePath + pFilename;
            CComPtr<IDxcBlobEncoding> source;
            HRESULT result = m_Library->CreateBlobFromFile(path.c_str(), nullptr, &source);

            if (SUCCEEDED(result) && ppIncludeSource)
                *ppIncludeSource = source.Detach();
            return result;
        }
    };

    const IKernelCompiler::DescriptorType& KernelCompiler::GetDesc() const
    {
        return m_Desc;
    }

    ResultCode KernelCompiler::Init(const IKernelCompiler::DescriptorType& desc)
    {
        m_Desc = desc;

        UN_VerifyResultFatal(DynamicLibrary::Create(&m_DynamicLibrary), "Couldn't create DynamicLibrary object");

        if (auto resultCode = m_DynamicLibrary->Init("dxcompiler"); !Succeeded(resultCode))
        {
            UN_Verify(false, "Couldn't load dxcompiler" UN_DLL_EXTENSION ", result code was {}", resultCode);
            return resultCode;
        }

        return ResultCode::Success;
    }

    ResultCode KernelCompiler::Compile(const KernelCompilerArgs& args, HeapArray<Byte>* pResult)
    {
        DxcCreateInstanceProc createInstance;
        UN_VerifyResultFatal(m_DynamicLibrary->GetFunction("DxcCreateInstance", &createInstance),
                             "Couldn't find DxcCreateInstance()");

        CComPtr<IDxcLibrary> pLibrary;
        HRESULT result = createInstance(CLSID_DxcLibrary, IID_PPV_ARGS(&pLibrary));
        if (FAILED(result))
        {
            UN_Error(false, "Couldn't create a DXC library");
            return ConvertResult(result);
        }

        CComPtr<IDxcCompiler> pCompiler;
        result = createInstance(CLSID_DxcCompiler, IID_PPV_ARGS(&pCompiler));
        if (FAILED(result))
        {
            UN_Error(false, "Couldn't create a DXC compiler");
            return ConvertResult(result);
        }

        CComPtr<IDxcBlobEncoding> source;
        auto sourceSize = static_cast<UInt32>(args.SourceCode.Length());
        result          = pLibrary->CreateBlobWithEncodingFromPinned(args.SourceCode.Data(), sourceSize, CP_UTF8, &source);
        if (FAILED(result))
        {
            UN_Error(false, "Couldn't create a DXC Blob encoding");
            return ConvertResult(result);
        }

        IncludeHandler includeHandler(L"", pLibrary);

        std::vector<DxcDefine> defines;
#if UN_DEBUG
        defines.push_back(DxcDefine{ L"UN_DEBUG", L"1" });
#else
        defines.push_back(DxcDefine{ L"UN_DEBUG", L"0" });
#endif

        auto defineCount = static_cast<UInt32>(defines.size());

        std::vector<LPCWSTR> compileArgs;
        if (m_Desc.TargetLang == KernelTargetLang::SpirV)
        {
            compileArgs.assign({ ConvertOptLevel(args.OptimizationLevel),
                                 DXC_ARG_PACK_MATRIX_COLUMN_MAJOR,
                                 L"-spirv",
                                 L"-fspv-target-env=vulkan1.1",
                                 L"-fspv-extension=KHR",
                                 L"-fspv-extension=SPV_GOOGLE_hlsl_functionality1",
                                 L"-fspv-extension=SPV_GOOGLE_user_type",
                                 L"-fvk-use-dx-layout",
                                 L"-fspv-extension=SPV_EXT_descriptor_indexing",
                                 L"-fspv-reflect" });
        }

        auto argsCount = static_cast<UInt32>(compileArgs.size());

        auto entryPoint = std::wstring(args.EntryPoint, args.EntryPoint + strlen(args.EntryPoint));
        auto profile    = GetTargetProfile(m_Desc.SourceLang);
        CComPtr<IDxcOperationResult> compileResult;
        result = pCompiler->Compile(source,
                                    L"KernelComputeShader",
                                    entryPoint.c_str(),
                                    profile,
                                    compileArgs.data(),
                                    argsCount,
                                    defines.data(),
                                    defineCount,
                                    &includeHandler,
                                    &compileResult);

        if (SUCCEEDED(result))
        {
            HRESULT status;
            if (SUCCEEDED(compileResult->GetStatus(&status)))
            {
                result = status;
            }
        }

        if (SUCCEEDED(result))
        {
            CComPtr<IDxcBlob> pByteCode;
            UN_Verify(SUCCEEDED(compileResult->GetResult(&pByteCode)), "Couldn't get compilation result");
            auto pBuffer = static_cast<Byte*>(pByteCode->GetBufferPointer());
            *pResult     = HeapArray<Byte>::CopyFrom(ArraySlice(pBuffer, pBuffer + pByteCode->GetBufferSize()));
            return ResultCode::Success;
        }
        else
        {
            CComPtr<IDxcBlobEncoding> errors;
            CComPtr<IDxcBlobEncoding> unicodeErrors;
            if (SUCCEEDED(compileResult->GetErrorBuffer(&errors)) && SUCCEEDED(pLibrary->GetBlobAsUtf8(errors, &unicodeErrors)))
            {
                auto errorString = static_cast<const char*>(unicodeErrors->GetBufferPointer());
                UN_VerifyError(false, "Shader compilation failed: {}", errorString);
            }

            return ConvertResult(result);
        }
    }
} // namespace UN
