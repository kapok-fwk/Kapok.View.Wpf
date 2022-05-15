using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Kapok.View.Wpf.Report;
using Kapok.Core;
using Kapok.Entity;
using Kapok.Report.DataModel;
using WindowViewModelRes = Kapok.View.Wpf.Resources.Window.WindowViewModel;

namespace Kapok.View.Wpf;

public interface IWpfViewDomain
{
}

public class WpfViewDomain : ViewDomain, IWpfViewDomain
{
    private static readonly Dictionary<Type, Func<Window>> PageWpfWindowConstructors = new();
    private static readonly Dictionary<Type, Type> PageWpfControlTypes = new();
    private readonly Dictionary<IPage, ICollection<IPage>> _pageContainer = new(); // key = owning page, value = collection with pages the page has in its container. A page can only have one container
    protected static readonly Dictionary<IPage, Window> PageWpfWindows = new();

    public static void RegisterPageWpfWindowConstructor<TPage>(Func<Window> constructWindow)
        where TPage : class, IPage
    {
        if (PageWpfWindowConstructors.ContainsKey(typeof(TPage)))
        {
            PageWpfWindowConstructors[typeof(TPage)] = constructWindow;
        }
        else
        {
            PageWpfWindowConstructors.Add(typeof(TPage), constructWindow);
        }
    }

    public static void RegisterPageWpfControlType<TPage>(Type controlType)
        where TPage : class, IPage
    {
        if (PageWpfControlTypes.ContainsKey(typeof(TPage)))
        {
            PageWpfControlTypes[typeof(TPage)] = controlType;
        }
        else
        {
            PageWpfControlTypes.Add(typeof(TPage), controlType);
        }
    }

    public WpfViewDomain(Action<int> shutdownApplicationAction)
    {
        ShutdownApplication = shutdownApplicationAction;
    }

    #region Configuration

    private static void TestTypeParameterlessPublicConstructor(Type type)
    {
        if (!type.IsClass)
            throw new NotSupportedException();

        var constructorInfo = type.GetConstructor(new Type[0]);
        if (constructorInfo == null)
            throw new NotSupportedException("The type must have an public parameterless constructor");
    }

    private Type _defaultDialogPageWindow = typeof(DialogPageWindow);
    private Type _defaultCardPageWindow = typeof(CardPageWindow);
    private Type _defaultListPageWindow = typeof(ListPageWindow);
    private Type _defaultPopupListPageWindow = typeof(PopupListPageWindow);

    public Type DefaultDialogPageWindow
    {
        get => _defaultDialogPageWindow;
        set
        {
            if (value == null)
                throw new ArgumentNullException();
            if (!typeof(Window).IsAssignableFrom(value))
                throw new NotSupportedException($"The default dialog page type must be assignable from type {typeof(Window).FullName}.");

            TestTypeParameterlessPublicConstructor(value);

            _defaultDialogPageWindow = value;
        }
    }

    public Type DefaultCardPageWindow
    {
        get => _defaultCardPageWindow;
        set
        {
            if (value == null)
                throw new ArgumentNullException();
            if (!typeof(Window).IsAssignableFrom(value))
                throw new NotSupportedException($"The default list page type must be assignable from type {typeof(Window).FullName}.");

            TestTypeParameterlessPublicConstructor(value);

            _defaultCardPageWindow = value;
        }
    }

    public Type DefaultListPageWindow
    {
        get => _defaultListPageWindow;
        set
        {
            if (value == null)
                throw new ArgumentNullException();
            if (!typeof(Window).IsAssignableFrom(value))
                throw new NotSupportedException($"The default list page type must be assignable from type {typeof(Window).FullName}.");

            TestTypeParameterlessPublicConstructor(value);

            _defaultListPageWindow = value;
        }
    }

    public Type DefaultPopupListPageWindow
    {
        get => _defaultPopupListPageWindow;
        set
        {
            if (value == null)
                throw new ArgumentNullException();
            if (!typeof(Window).IsAssignableFrom(value))
                throw new NotSupportedException($"The default popup list page type must be assignable from type {typeof(Window).FullName}.");

            TestTypeParameterlessPublicConstructor(value);

            _defaultPopupListPageWindow = value;
        }
    }

    #endregion

    public override Type GetPageControlType(Type pageType)
    {
        if (PageWpfControlTypes.ContainsKey(pageType))
            return PageWpfControlTypes[pageType];

        if (typeof(IListPage).IsAssignableFrom(pageType))
        {
            return typeof(ListPageControl);
        }
        if (typeof(ICardPage).IsAssignableFrom(pageType))
        {
            return typeof(CardPageControl);
        }
        return typeof(BlankDefaultPageControl);
    }

    private Window ConstructWindow(Type pageType)
    {
        if (PageWpfWindowConstructors.ContainsKey(pageType))
            return PageWpfWindowConstructors[pageType].Invoke();

        if (typeof(QuestionDialogPage).IsAssignableFrom(pageType))
            return new QuestionDialogPageWindow();

        if (typeof(IListPage).IsAssignableFrom(pageType))
            return (Window) Activator.CreateInstance(DefaultListPageWindow);
        if (typeof(IDialogPage).IsAssignableFrom(pageType))
            return (Window) Activator.CreateInstance(DefaultDialogPageWindow);
        if (typeof(ICardPage).IsAssignableFrom(pageType))
            return (Window) Activator.CreateInstance(DefaultCardPageWindow);

        throw new NotSupportedException($"No WPF window defined for page {pageType.FullName}");
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private void CheckWindowAlreadyOpen(IPage page)
    {
        if (PageWpfWindows.ContainsKey(page))
            throw new NotSupportedException("The page is already opened.");
        if (_pageContainer.Values.FirstOrDefault(pc => pc.Contains(page)) != null)
            throw new NotSupportedException("The page is already opened in a page container.");
    }

    private void ConstructPageWindow(IPage page)
    {
        var newWindow = ConstructWindow(page.GetType());
        newWindow.DataContext = page;

        // subscribe events
        newWindow.Closing += Window_Closing;
        newWindow.Closed += Window_Closed;

        PageWpfWindows.Add(page, newWindow);
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (sender is Window window)
        {
            var page = PageWpfWindows.FirstOrDefault(pair => pair.Value == window).Key;
            // TODO: add debug assert when `page == null`; should never happen

            if (page != null)
            {
                if (page is IDialogPage dialogPage)
                {
                    window.DialogResult = dialogPage.DialogResult;
                }

                if (page is Kapok.View.Page pageObject)
                {
                    pageObject.RaiseClosing(e);
                }
            }
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        if (sender is Window window)
        {
            var page = PageWpfWindows.FirstOrDefault(pair => pair.Value == window).Key;
            // TODO: add debug assert when `page == null`; should never happen

            if (page != null)
            {
                if (page is Kapok.View.Page pageObject)
                {
                    pageObject.RaiseClosed();
                }

                PageWpfWindows.Remove(page);
            }

            // unsubscribe events
            window.Closing -= Window_Closing;
            window.Closed -= Window_Closed;
        }
    }

    public override void ShowPage(IPage page)
    {
        CheckWindowAlreadyOpen(page);
        ConstructPageWindow(page);
        PageWpfWindows[page].Show();
    }

    private Window GetOwnerWindow(IPage ownerPage)
    {
        var owingPageContainer = (
            from p in _pageContainer
            where p.Value.Contains(ownerPage)
            select p.Key
        ).FirstOrDefault();
        if (owingPageContainer != null)
        {
            // TODO: here we do not have a recursion check to prevent endless loops!
            return GetOwnerWindow(owingPageContainer);
        }

        if (!PageWpfWindows.ContainsKey(ownerPage))
            throw new ArgumentException("The owner page does not have an active, open window.", nameof(ownerPage));

        return PageWpfWindows[ownerPage];
    }

    public override bool? ShowDialogPage(IPage page, IPage? ownerPage = null)
    {
        CheckWindowAlreadyOpen(page);
        ConstructPageWindow(page);

        if (ownerPage != null)
        {
            PageWpfWindows[page].Owner = GetOwnerWindow(ownerPage);
        }

        return PageWpfWindows[page].ShowDialog();
    }

    public override void RegisterPageContainer(IPage owningPage, ICollection<IPage> pageContainer)
    {
        if (_pageContainer.ContainsKey(owningPage))
            throw new ArgumentException("There is already a page container registered for this page", nameof(owningPage));
        if (_pageContainer.ContainsValue(pageContainer))
            throw new ArgumentException("The page container is already for another page", nameof(pageContainer));

        _pageContainer.Add(owningPage, pageContainer);
    }

    public override void UnregisterPageContainer(IPage owningPage)
    {
        if (!_pageContainer.ContainsKey(owningPage))
        {
#if DEBUG
                throw new ArgumentException("There is no page container registered for this page", nameof(owningPage));
#else
            return;
#endif
        }

        _pageContainer.Remove(owningPage);
    }

    public override void ClosePage(IPage page)
    {
        var pageInPageContainer = _pageContainer.Values.FirstOrDefault(pc => pc.Contains(page));
        if (pageInPageContainer != null)
        {
            if (page is Kapok.View.Page pageObject)
            {
                pageObject.RaiseClosed();
            }
            pageInPageContainer.Remove(page);
            return;
        }

        if (!PageWpfWindows.ContainsKey(page))
        {
            // we use optimistic behavior here; the page will be closed nevertheless
#if DEBUG
                Debug.WriteLine("The page does not have an window registered to it, can not close the window!");
#endif
            return;
        }

        PageWpfWindows[page].Close();
    }

    public override IQueryableView<TEntity> CreateQueryableView<TEntity>(IQueryable<TEntity> queryable)
    {
        var view = new QueryableCollectionViewSource<TEntity>();
        view.QueryableSource = queryable;
        return view;
    }

    public override IPropertyLookupView CreatePropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain, IDataSetView? dataSet)
    {
        return new PropertyLookupView(lookupDefinition, dataDomain, dataSet);
    }

    public override IDataSetView<TEntry> CreateDataSetView<TEntry>(IDataDomainScope dataDomainScope, IDao<TEntry>? repository = null)
    {
        return new WpfDataSetView<TEntry>(this, dataDomainScope, repository);
    }

    public override IHierarchyDataSetView<TEntry> CreateHierarchyDataSetView<TEntry>(IDataDomainScope dataDomainScope, IDao<TEntry>? repository = null)
    {
        return new HierarchyDataSetView<TEntry>(this, dataDomainScope, repository);
    }

    public override void ShowInfoMessage(string message, string? title = null, IPage? ownerPage = null)
    {
        if (title == null)
            title = WindowViewModelRes.InfoGuiMessage_Caption;

        if (ownerPage != null)
        {
            MessageBox.Show(GetOwnerWindow(ownerPage), message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    public override void ShowErrorMessage(string message, string? title = null, IPage? ownerPage = null, Exception? exception = null)
    {
        if (exception != null)
        {
            var text = new StringBuilder();
            text.AppendFormat(WindowViewModelRes.ExceptionGuiMessage_WithException_Text, message, exception.Message,
                exception.StackTrace);
            var innerException = exception.InnerException;
            while (innerException != null)
            {
                text.AppendFormat(WindowViewModelRes.ExceptionGuiMessage_InnerExceptionText, innerException.Message,
                    innerException.StackTrace);

                innerException = innerException.InnerException;
            }

            message = text.ToString();
        }

        if (title == null)
            title = WindowViewModelRes.ExceptionGuiMessage_Caption;

        if (ownerPage != null)
        {
            MessageBox.Show(GetOwnerWindow(ownerPage), message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public override bool ShowYesNoQuestionMessage(string message, string? title = null, IPage? ownerPage = null)
    {
        if (title == null)
            title = WindowViewModelRes.YesNoQuestionGuiMessage_Caption;

        MessageBoxResult result;

        if (ownerPage != null)
        {
            result = MessageBox.Show(GetOwnerWindow(ownerPage), message, title, MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
        }
        else
        {
            result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
        }

        return result == MessageBoxResult.Yes;
    }

    public override bool ShowConfirmMessage(string message, string? title = null, IPage? ownerPage = null)
    {
        if (title == null)
            title = WindowViewModelRes.ConfirmGuiMessage_Caption;

        MessageBoxResult result;

        if (ownerPage != null)
        {
            result = MessageBox.Show(GetOwnerWindow(ownerPage), message, title, MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
        }
        else
        {
            result = MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Asterisk);
        }

        return result == MessageBoxResult.OK;
    }

    /// <summary>
    /// A page is rendered in a content control.
    /// 
    /// This method returns the content control of the given page.
    /// </summary>
    /// <param name="page">
    /// The page you want to retrieve the content control from.
    /// </param>
    /// <returns>
    /// Returns the ContentControl of the page.
    /// When currently not rendered, null is returned.
    /// </returns>
    protected virtual ContentControl? GetPageContentControl(IPage? page)
    {
        if (page == null)
            return null;

        var pageContainerPage = _pageContainer.FirstOrDefault(p => p.Value.Contains(page)).Key;
        if (pageContainerPage != null)
        {
            return GetPageContentControlFromPageContainerPage(page, pageContainerPage);
        }
            
        if (PageWpfWindows.ContainsKey(page))
        {
            var pageControlType = GetPageControlType(page.GetType());
            return PageWpfWindows[page].FindVisualChild(pageControlType) as ContentControl;
        }

        return null;
    }

    // NOTE: not supported by default --> needs to be implemented by a sub-ViewDomain implementation
    protected virtual ContentControl? GetPageContentControlFromPageContainerPage(
        IPage page, IPage pageContainerPage)
    {
        return null;
    }

    private DataGrid? GetPageDefaultDataGrid(IPage page)
    {
        var pageContentControl = GetPageContentControl(page);
        if (pageContentControl == null)
            throw new ArgumentException("The ContentControl of page could not be found. Probably the page is not active or has no open window.", nameof(page));

        return pageContentControl.FindVisualChild<DataGrid>("TableDataDataGrid");
    }

    public override void PageEndEdit(IPage page)
    {
        var pageContentControl = GetPageContentControl(page);
        if (pageContentControl == null)
            throw new ArgumentException("The ContentControl of page could not be found. Probably the page is not active or has no open window.", nameof(page));

        // Make sure that every object with a binding
        // is saved to the view model before we start saving.

        // ReSharper disable once SuspiciousTypeConversion.Global
        var focusedDependencyObject = FocusManager.GetFocusedElement(pageContentControl) as DependencyObject;

        // sometimes the focused element is not a element which is a valid type for .EndEdit(), so we search here for the DependencyObject which can be used if this is the case.
        while (focusedDependencyObject != null && focusedDependencyObject != pageContentControl && !(focusedDependencyObject is Visual || focusedDependencyObject is Visual3D))
        {
            focusedDependencyObject = LogicalTreeHelper.GetParent(focusedDependencyObject);
        }

        focusedDependencyObject?.EndEdit();
    }

    public override void StartEditingDefaultDataGridCurrentEntity(IDataPage page, bool enforceFirstEditableRow)
    {
        var dataGrid = GetPageDefaultDataGrid(page);
        if (dataGrid == null)
            return;

        DataGridColumn? columnToEdit = null;

        if (!enforceFirstEditableRow)
        {
            columnToEdit = dataGrid.CurrentColumn;
        }
        if (columnToEdit == null)
        {
            columnToEdit = dataGrid.Columns.FirstOrDefault(c => c.Visibility == Visibility.Visible && !c.IsReadOnly);
            if (columnToEdit == null)
                return;
        }

        // BUG: minor UX issue: this only works after the user has manually selected a column after all data has been loaded.
        dataGrid.CurrentCell = new DataGridCellInfo(page.DataSet.Current, columnToEdit);
        dataGrid.BeginEdit();
    }

    public override string? OpenOpenFileDialog(string title, string fileMask, IPage? ownerPage = null)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog();
        dialog.Title = title;
        dialog.Filter = fileMask;
        dialog.Multiselect = false;
        var result = ownerPage == null ? dialog.ShowDialog() : dialog.ShowDialog(GetOwnerWindow(ownerPage));

        if (!result.HasValue || result.Value == false)
            return null;

        return dialog.FileName;
    }

    public override string? OpenSaveFileDialog(string title, string fileMask, IPage? ownerPage = null)
    {
        var dialog = new Microsoft.Win32.SaveFileDialog();
        dialog.Title = title;
        dialog.Filter = fileMask;
        var result = ownerPage == null ? dialog.ShowDialog() : dialog.ShowDialog(GetOwnerWindow(ownerPage));

        if (!result.HasValue || result.Value == false)
            return null;

        return dialog.FileName;
    }

    public override bool OpenReportDialog(object model, IDataDomain dataDomain, object? reportLayout = null, IPage? ownerPage = null)
    {
        if (!(model is Kapok.Report.Model.Report reportModel))
            throw new ArgumentException(
                $"The parameter {nameof(model)} must be assignable to the type {typeof(Kapok.Report.Model.Report).FullName}.");

        IPage page;

        if (model is Kapok.Report.Model.LinqQueryableReport linqQueryableReport)
        {
            page = new LinqQueryableReportPage(linqQueryableReport, this);
        }
        else if (model is Kapok.Report.Model.MultipleDataTableReport multipleDataTableReport)
        {
            page = new MultipleDataTableReportPage(multipleDataTableReport, this);
        }
        else
        {
            page = new MimeTypeReportPage(
                reportModel,
                dataDomain,
                reportLayout as ReportLayout,
                this);
        }

        var result = ownerPage == null ? page.ShowDialog() : page.ShowDialog(ownerPage);
        return result ?? false;
    }

    public override void OpenFile(string filename)
    {
        Process.Start(new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = filename
        });
    }

    public override void BusinessLayerMessageEventToSingleUIMessage(object businessLayerObject, ReportBusinessLayerMessageEventArgs eventArgs)
    {
        switch (eventArgs.Message.Severity)
        {
            case MessageSeverity.Info:
                ShowInfoMessage(eventArgs.Message.Text);
                break;
            case MessageSeverity.Warning:
                ShowInfoMessage(eventArgs.Message.Text, "Warning"); // TODO translation missing
                break;
            case MessageSeverity.Error:
                ShowErrorMessage(eventArgs.Message.Text);
                break;
        }
    }
}