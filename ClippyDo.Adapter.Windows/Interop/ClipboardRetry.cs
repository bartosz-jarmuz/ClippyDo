namespace ClippyDo.Adapter.Windows.Interop;

/// <summary>
/// Short capped backoff for transient OLE clipboard contention.
/// </summary>
internal static class ClipboardRetry
{
    public static T Run<T>(Func<T> action, int attempts = 6, int initialDelayMs = 8)
    {
        var delay = initialDelayMs;
        for (int i = 0; i < attempts; i++)
        {
            try { return action(); }
            catch when (i < attempts - 1)
            {
                Thread.Sleep(delay);
                delay = Math.Min(delay * 2, 64);
            }
        }
        return action(); // bubble final
    }
}
