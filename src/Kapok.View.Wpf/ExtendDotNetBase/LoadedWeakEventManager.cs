namespace System.Windows;

/// <summary>
/// Provides a <see cref="WeakEventManager"/> implementation so that you can use the "weak event listener" pattern to attach listeners for
/// the <seealso cref="FrameworkElement.Unloaded"/> event.
/// </summary>
public class LoadedWeakEventManager : WeakEventManager
{
    private LoadedWeakEventManager()
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
    private static LoadedWeakEventManager CurrentManager
    {
        get
        {
            Type managerType = typeof(LoadedWeakEventManager);
            LoadedWeakEventManager? manager =
                (LoadedWeakEventManager?)GetCurrentManager(managerType);

            // at first use, create and register a new manager
            if (manager == null)
            {
                manager = new LoadedWeakEventManager();
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
        typedSource.Loaded += new RoutedEventHandler(OnLoaded);
    }

    /// <summary>
    /// Stop listening to the given source for the event.
    /// </summary>
    protected override void StopListening(object source)
    {
        FrameworkElement typedSource = (FrameworkElement)source;
        typedSource.Loaded -= new RoutedEventHandler(OnLoaded);
    }

    /// <summary>
    /// Event handler for the SomeEvent event.
    /// </summary>
    void OnLoaded(object? sender, RoutedEventArgs e)
    {
        DeliverEvent(sender, e);
    }
}