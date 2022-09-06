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
    public IntPtr Handle { get; private set; }

    protected NativeObject(IntPtr handle)
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
        _ = IObject_Release(Handle);
        Handle = IntPtr.Zero;
    }

    protected virtual void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
        }
    }

    [DllImport("UnCompute")]
    private static extern uint IObject_AddRef(IntPtr self);

    [DllImport("UnCompute")]
    private static extern uint IObject_Release(IntPtr self);

    ~NativeObject()
    {
        Dispose(false);
    }
}
