using System.Runtime.InteropServices;

namespace UraniumCompute.Memory;

public abstract class UnObject : IDisposable
{
    public IntPtr Handle { get; private set; }

    protected UnObject(IntPtr handle)
    {
        Handle = handle;
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~UnObject()
    {
        Dispose(false);
    }
    
    [DllImport("UnCompute")]
    private static extern uint IObject_AddRef(IntPtr self);

    [DllImport("UnCompute")]
    private static extern uint IObject_Release(IntPtr self);
}
