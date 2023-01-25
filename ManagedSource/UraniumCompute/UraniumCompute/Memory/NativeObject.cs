using System.Runtime.InteropServices;
using UraniumCompute.Utils;

namespace UraniumCompute.Memory;

/// <summary>
///     Base of all objects in the library that are implemented in the unmanaged code.
///     Should not be derived by the user classes.
/// </summary>
public abstract class NativeObject : IDisposable
{
    /// <summary>
    ///     A pointer to the native object.
    /// </summary>
    public nint Handle { get; private set; }

    protected NativeObject(nint handle)
    {
        Handle = handle;
    }

    public override string ToString()
    {
        return $"{{ {GetType().GetCSharpName()}: {nameof(NativeObject)} at 0x{Handle:x} }}";
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources()
    {
        if (Handle == nint.Zero)
        {
            return;
        }

        _ = IObject_Release(Handle);
        Handle = nint.Zero;
    }

    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
        }
    }

    protected void IncrementReferenceCount()
    {
        _ = IObject_AddRef(Handle);
    }

    [DllImport("UnCompute")]
    private static extern uint IObject_AddRef(nint self);

    [DllImport("UnCompute")]
    private static extern uint IObject_Release(nint self);

    ~NativeObject()
    {
        Dispose(false);
    }
}
