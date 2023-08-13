namespace System.Windows;

/// <summary>
/// Provides a <see cref="WeakEventManager"/> implementation so that you can use the "weak event listener" pattern to attach listeners for
/// the <seealso cref="FrameworkElement.Unloaded"/> event.
/// </summary>
public class UnloadedWeakEventManager : WeakEventManager
{
    private UnloadedWeakEventManager()
    {
    }

    /// <summary>
    /// Add a handler for the given source's event.
    /// </summary>
    public static void AddHandler(FrameworkElement source,
        EventHandler<RoutedEventArgs> handler)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        CurrentManager.ProtectedAddHandler(source, handler);
    }

    /// <summary>
    /// Remove a handler for the given source's event.
    /// </summary>
    public static void RemoveHandler(FrameworkElement source,
        EventHandler<RoutedEventArgs> handler)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        CurrentManager.ProtectedRemoveHandler(source, handler);
    }

    /// <summary>
    /// Get the event manager for the current thread.
    /// </summary>
    private static UnloadedWeakEventManager CurrentManager
    {
        get
        {
            Type managerType = typeof(UnloadedWeakEventManager);
            UnloadedWeakEventManager? manager =
                (UnloadedWeakEventManager?)GetCurrentManager(managerType);

            // at first use, create and register a new manager
            if (manager == null)
            {
                manager = new UnloadedWeakEventManager();
                SetCurrentManager(managerType, manager);
            }

            return manager;
        }
    }

    /// <summary>
    /// Return a new list to hold listeners to the event.
    /// </summary>
    protected override ListenerList NewListenerList()
    {
        return new ListenerList<RoutedEventArgs>();
    }

    /// <summary>
    /// Listen to the given source for the event.
    /// </summary>
    protected override void StartListening(object source)
    {
        FrameworkElement typedSource = (FrameworkElement)source;
        typedSource.Unloaded += new RoutedEventHandler(OnUnloaded);
    }

    /// <summary>
    /// Stop listening to the given source for the event.
    /// </summary>
    protected override void StopListening(object source)
    {
        FrameworkElement typedSource = (FrameworkElement)source;
        typedSource.Unloaded -= new RoutedEventHandler(OnUnloaded);
    }

    /// <summary>
    /// Event handler for the SomeEvent event.
    /// </summary>
    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        DeliverEvent(sender, e);
    }
}