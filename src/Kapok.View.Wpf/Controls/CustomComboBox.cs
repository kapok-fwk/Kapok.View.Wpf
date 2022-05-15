using System.Windows.Controls;
using System.Windows.Input;

namespace Kapok.View.Wpf;

// TODO: this class name is ugly
// developed with help of: https://stackoverflow.com/questions/2963462/how-to-add-a-focus-to-an-editable-combobox-in-wpf
public class CustomComboBox : ComboBox
{
    private bool _hasKeyboardFocus;
    TextBox _textBox;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _textBox = Template.FindName("PART_EditableTextBox", this) as TextBox;
        if (_hasKeyboardFocus && _textBox != null)
        {
            // it doesn't work without using the dispatcher.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                _textBox.Focus();
                _textBox.SelectionStart = _textBox.Text.Length; // set caret to end of text
            }));
        }
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);
        _hasKeyboardFocus = false;
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);
        _hasKeyboardFocus = true;
    }
}