using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace XtraLibrary.Threading
{
    public struct TimedLock: IDisposable
{
    public static TimedLock Lock (object o)
    {
        return Lock (o, TimeSpan.FromSeconds (10));
    }

    public static TimedLock Lock (object o, TimeSpan timeout)
    {
        TimedLock tl = new TimedLock (o);
        if (!Monitor.TryEnter(o, timeout))
        {
#if DEBUG
            System.GC.SuppressFinalize(tl.m_LeakDetector);
#endif
            throw new LockTimeoutException ();
        }

        return tl;
    }

    private TimedLock (object o)
    {
        m_Target = o;
#if DEBUG
        m_LeakDetector = new Sentinel();
#endif
    }
    private object m_Target;

    public void Dispose ()
    {
        Monitor.Exit (m_Target);

        // It's a bad error if someone forgets to call Dispose,
        // so in Debug builds, we put a finalizer in to detect
        // the error. If Dispose is called, we suppress the
        // finalizer.
#if DEBUG
        GC.SuppressFinalize(m_LeakDetector);
#endif
    }

#if DEBUG
    // (In Debug mode, we make it a class so that we can add a finalizer
    // in order to detect when the object is not freed.)
    private class Sentinel
    {
        ~Sentinel()
        {
            // If this finalizer runs, someone somewhere failed to
            // call Dispose, which means we've failed to leave
            // a monitor!
            System.Diagnostics.Debug.Fail("Undisposed lock");
        }
    }
    private Sentinel m_LeakDetector;
#endif

}
public class LockTimeoutException : ApplicationException
{
    public LockTimeoutException () : base("Timeout waiting for lock")
    {
    }
}
}
