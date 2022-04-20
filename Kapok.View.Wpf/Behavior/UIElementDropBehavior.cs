using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xaml.Behaviors;

// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Kapok.View.Wpf;

// source: https://putridparrot.com/blog/a-wpf-dragdrop-target-behavior/

public interface IDropTarget
{
    bool CanDrop(object source, IDataObject data);
    void Drop(object source, IDataObject data);
}

public class UIElementDropBehavior : Behavior<UIElement>
{
    private AdornerManager _adornerManager;

    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.AllowDrop = true;
        AssociatedObject.DragEnter += AssociatedObject_DragEnter;
        AssociatedObject.DragOver += AssociatedObject_DragOver;
        AssociatedObject.DragLeave += AssociatedObject_DragLeave;
        AssociatedObject.Drop += AssociatedObject_Drop;
        AssociatedObject.PreviewKeyDown += AssociatedObjectOnKeyDown;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject.AllowDrop = false;
        AssociatedObject.DragEnter -= AssociatedObject_DragEnter;
        AssociatedObject.DragOver -= AssociatedObject_DragOver;
        AssociatedObject.DragLeave -= AssociatedObject_DragLeave;
        AssociatedObject.Drop -= AssociatedObject_Drop;
        AssociatedObject.PreviewKeyDown -= AssociatedObjectOnKeyDown;
    }

    private void AssociatedObjectOnKeyDown(object sender, KeyEventArgs e)
    {
        if ((e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) ||
            (e.Key == Key.V) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
        {
            var data = Clipboard.GetDataObject();
            if (CanAccept(sender, data))
            {
                Drop(sender, data);
            }
        }
    }

    private void AssociatedObject_Drop(object sender, DragEventArgs e)
    {
        if (CanAccept(sender, e.Data))
        {
            Drop(sender, e.Data);
        }

        _adornerManager?.Remove();
        e.Handled = true;
    }

    private void AssociatedObject_DragLeave(object sender, DragEventArgs e)
    {
        if (_adornerManager != null)
        {
            if (sender is IInputElement inputElement)
            {
                var pt = e.GetPosition(inputElement);

                if (sender is UIElement element)
                {
                    if (!pt.Within(element.RenderSize) || e.KeyStates == DragDropKeyStates.None)
                    {
                        _adornerManager.Remove();
                    }
                }
            }
        }
        e.Handled = true;
    }

    private void AssociatedObject_DragOver(object sender, DragEventArgs e)
    {
        if (CanAccept(sender, e.Data))
        {
            e.Effects = DragDropEffects.Copy;

            if (_adornerManager != null)
            {
                if (sender is UIElement element)
                {
                    _adornerManager.Update(element);
                }
            }
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }
        e.Handled = true;
    }

    private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
    {
        if (_adornerManager == null)
        {
            if (sender is UIElement element)
            {
                _adornerManager = new AdornerManager(AdornerLayer.GetAdornerLayer(element), adornedElement => new UIElementDropAdorner(adornedElement));
            }
        }
        e.Handled = true;
    }

    private bool CanAccept(object sender, IDataObject data)
    {
        if (sender is FrameworkElement element && element.DataContext != null)
        {
            if (element.DataContext is IDropTargetOnPage dropTargetOnPage)
            {
                if (data == null)
                    return false;

                if (data.GetFormats().Contains(DataFormats.FileDrop))
                {
                    if (data.GetData(DataFormats.FileDrop) is string[] files)
                    {
                        return dropTargetOnPage.CanDropFile(files);
                    }

                    return false;
                }
            }

            if (element.DataContext is IDropTarget dropTarget)
            {
                if (dropTarget.CanDrop(data.GetData("DragSource"), data))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void Drop(object sender, IDataObject data)
    {
        if (sender is FrameworkElement element && element.DataContext != null)
        {
            if (element.DataContext is IDropTargetOnPage targetOnPage)
            {
                if (data.GetFormats().Contains(DataFormats.FileDrop))
                {
                    var files = (string[]) data.GetData(DataFormats.FileDrop);
                    targetOnPage.DropFile(files);
                }
            }

            if (element.DataContext is IDropTarget target)
            {
                target.Drop(data.GetData("DragSource"), data);
            }
        }
    }
}

public class AdornerManager
{
    private readonly AdornerLayer adornerLayer;
    private readonly Func<UIElement, Adorner> adornerFactory;

    private Adorner adorner;

    public AdornerManager(
        AdornerLayer adornerLayer,
        Func<UIElement, Adorner> adornerFactory)
    {
        this.adornerLayer = adornerLayer;
        this.adornerFactory = adornerFactory;
    }

    public void Update(UIElement adornedElement)
    {
        if (adorner == null || !adorner.AdornedElement.Equals(adornedElement))
        {
            Remove();
            adorner = adornerFactory(adornedElement);
            adornerLayer.Add(adorner);
            adornerLayer.Update(adornedElement);
            adorner.Visibility = Visibility.Visible;
        }
    }

    public void Remove()
    {
        if (adorner != null)
        {
            adorner.Visibility = Visibility.Collapsed;
            adornerLayer.Remove(adorner);
            adorner = null;
        }
    }
}

public class UIElementDropAdorner : Adorner
{
    public UIElementDropAdorner(UIElement adornedElement) :
        base(adornedElement)
    {
        Focusable = false;
        IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        const int PEN_WIDTH = 1;

        var adornedRect = new Rect(AdornedElement.RenderSize);

        var brush = new SolidColorBrush(Colors.White);
        brush.Opacity = 0.5;

        drawingContext.DrawRectangle(brush,
            new Pen(Brushes.LightGray, PEN_WIDTH),
            adornedRect);

        var image = new BitmapImage(
            new Uri(ImageManager.GetImageResource("document-arrow-down", ImageManager.ImageSize.Large),
                UriKind.Absolute));

        var typeface = new Typeface(
            new FontFamily("Segoe UI"),
            FontStyles.Normal,
            FontWeights.Normal, FontStretches.Normal);
        var formattedText = new FormattedText(
            "Drop files here", // TODO translation required
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            typeface,
            24,
            Brushes.LightGray,
            VisualTreeHelper.GetDpi(AdornedElement).PixelsPerDip);

        var centre = new Point(
            AdornedElement.RenderSize.Width / 2,
            AdornedElement.RenderSize.Height / 2);

        var top = centre.Y - (image.Height + formattedText.Height) / 2;
        var textLocation = new Point(
            centre.X - formattedText.WidthIncludingTrailingWhitespace / 2,
            top + image.Height);

        drawingContext.DrawImage(image,
            new Rect(centre.X - image.Width / 2,
                top,
                image.Width,
                image.Height));
        drawingContext.DrawText(formattedText, textLocation);
    }
}