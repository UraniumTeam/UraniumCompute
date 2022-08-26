using System.Runtime.InteropServices;

namespace UraniumCompute.Memory;

/// <summary>
///     Base of all objects in the library. Should not be derived by the user code.
/// </summary>
public abstract class UnObject : IDisposable
{
    /// <summary>
    ///     A pointer to the native object.
    /// </summary>
    public IntPtr Handle { get; private set; }

    protected UnObject(IntPtr handle)
    {
        Handle = handle;
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

    ~UnObject()
    {
        Dispose(false);
    }
}
