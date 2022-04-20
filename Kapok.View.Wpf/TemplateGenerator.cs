using System.Windows;
using System.Windows.Controls;

namespace Kapok.View.Wpf;
// source: https://www.codeproject.com/Tips/808808/Create-Data-and-Control-Templates-using-Delegates

/// <summary>
/// Class that helps the creation of control and data templates by using delegates.
/// </summary>
public static class TemplateGenerator
{
    // ReSharper disable once InconsistentNaming
    private sealed class _TemplateGeneratorControl : ContentControl
    {
        internal static readonly DependencyProperty FactoryProperty = DependencyProperty.Register("Factory",
            typeof(Func<object>), typeof(_TemplateGeneratorControl), new PropertyMetadata(null, _FactoryChanged));

        private static void _FactoryChanged(DependencyObject instance, DependencyPropertyChangedEventArgs args)
        {
            var control = (_TemplateGeneratorControl) instance;
            var factory = (Func<object>?) args.NewValue;
            if (factory == null)
                return;

            control.Content = factory();
        }
    }

    /// <summary>
    /// Creates a data-template that uses the given delegate to create new instances.
    /// </summary>
    public static DataTemplate CreateDataTemplate(Func<object> factory, bool sealTemplate = true)
    {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var frameworkElementFactory = new FrameworkElementFactory(typeof(_TemplateGeneratorControl));
        frameworkElementFactory.SetValue(_TemplateGeneratorControl.FactoryProperty, factory);

        var dataTemplate = new DataTemplate(typeof(DependencyObject));
        dataTemplate.VisualTree = frameworkElementFactory;
        if (sealTemplate)
            dataTemplate.Seal();
        return dataTemplate;
    }

    /// <summary>
    /// Creates a control-template that uses the given delegate to create new instances.
    /// </summary>
    public static ControlTemplate CreateControlTemplate(Type controlType, Func<object> factory, bool sealTemplate = true)
    {
        if (controlType == null)
            throw new ArgumentNullException(nameof(controlType));

        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var frameworkElementFactory = new FrameworkElementFactory(typeof(_TemplateGeneratorControl));
        frameworkElementFactory.SetValue(_TemplateGeneratorControl.FactoryProperty, factory);

        var controlTemplate = new ControlTemplate(controlType);
        controlTemplate.VisualTree = frameworkElementFactory;
        if (sealTemplate)
            controlTemplate.Seal();
        return controlTemplate;
    }
}