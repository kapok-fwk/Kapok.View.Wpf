using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Kapok.View.Wpf;

public class PopupThumbResizeBehavior : Behavior<Thumb>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject.DragDelta += OnDragDelta;
        AssociatedObject.DragStarted += OnDragStarted;
        AssociatedObject.DragCompleted += OnDragCompleted;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.DragDelta -= OnDragDelta;
        AssociatedObject.DragStarted -= OnDragStarted;
        AssociatedObject.DragCompleted -= OnDragCompleted;

        base.OnDetaching();
    }

    protected virtual void OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        Thumb t = AssociatedObject;

        var popup = t.FindLogicalParent<Popup>();
        if (popup == null)
            return;

        if (t.Cursor == Cursors.SizeWE
            || t.Cursor == Cursors.SizeNWSE)
        {
            popup.Width = Math.Min(t.MaxWidth,
                Math.Max(popup.Width + e.HorizontalChange,
                    t.MinWidth));
        }

        if (t.Cursor == Cursors.SizeNS
            || t.Cursor == Cursors.SizeNWSE)
        {
            popup.Height = Math.Min(t.MaxHeight,
                Math.Max(popup.Height + e.VerticalChange,
                    t.MinHeight));
        }
    }

    protected virtual void OnDragStarted(object sender, DragStartedEventArgs e)
    {
        //This is called when the user
        //starts dragging the thumb
    }

    protected virtual void OnDragCompleted(object sender, DragCompletedEventArgs e)
    {
        //This is called when the user
        //finishes dragging the thumb
    }
}