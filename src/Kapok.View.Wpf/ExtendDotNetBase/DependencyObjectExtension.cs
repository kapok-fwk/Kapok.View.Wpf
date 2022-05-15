using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Media3D;

// ReSharper disable once CheckNamespace
namespace System.Windows;

public static class DependencyObjectExtension
{
    /// <summary>
    /// Updates the binding for the object and each child object.
    ///
    /// Source: https://stackoverflow.com/questions/888324/endedit-equivalent-in-wpf
    /// </summary>
    /// <param name="parent"></param>
    public static void EndEdit(this DependencyObject? parent)
    {
        if (!(parent is Visual || parent is Visual3D))
            throw new ArgumentException("The parameter parent must have an DependencyObject which is of the type Visual or Visual3D.", nameof(parent));

        LocalValueEnumerator localValues = parent.GetLocalValueEnumerator();
        while (localValues.MoveNext())
        {
            LocalValueEntry entry = localValues.Current;
            if (BindingOperations.IsDataBound(parent, entry.Property))
            {
                BindingExpression binding = BindingOperations.GetBindingExpression(parent, entry.Property);
                if (binding != null && binding.IsDirty)
                {
                    binding.UpdateSource();
                }
            }
        }            

        for(int i=0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);

            if (child is Visual || child is Visual3D)
                EndEdit(child);
        }
    }

    /// <summary>
    /// Finds a Child of a given item in the visual tree.
    ///
    /// Source: https://stackoverflow.com/questions/636383/how-can-i-find-wpf-controls-by-name-or-type/9229255
    /// </summary>
    /// <param name="parent">A direct parent of the queried item.</param>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <param name="childName">x:Name or Name of child. If null or empty the first child will be given.</param>
    /// <returns>The first parent item that matches the submitted type parameter. 
    /// If not matching item can be found, 
    /// a null parent is being returned.</returns>
    public static T? FindVisualChild<T>(this DependencyObject? parent, string childName = "")
        where T : DependencyObject
    {    
        // Confirm parent and childName are valid. 
        if (parent == null) return null;

        T? foundChild = null;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // If the child is not of the request child type child
            T? childType = child as T;
            if (childType == null)
            {
                // recursively drill down the tree
                foundChild = FindVisualChild<T>(child, childName);

                // If the child is found, break so we do not overwrite the found child. 
                if (foundChild != null) break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                var frameworkElement = child as FrameworkElement;
                // If the child's name is set for search
                if (frameworkElement != null && frameworkElement.Name == childName)
                {
                    // if the child's name is of the request name
                    foundChild = (T)child;
                    break;
                }
            }
            else
            {
                // child element found.
                foundChild = (T)child;
                break;
            }
        }

        return foundChild;
    }

    // The same as FindVisualChild<T>; just without generic parameter
    public static DependencyObject? FindVisualChild(this DependencyObject? parent, Type childType, string childName = "")
    {    
        // Confirm parent and childName are valid. 
        if (parent == null) return null;

        DependencyObject? foundChild = null;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);

        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            // If the child is not of the request child type child
                
            if (!childType.IsInstanceOfType(child))
            {
                // recursively drill down the tree
                foundChild = FindVisualChild(child, childType, childName);

                // If the child is found, break so we do not overwrite the found child. 
                if (foundChild != null) break;
            }
            else if (!string.IsNullOrEmpty(childName))
            {
                var frameworkElement = child as FrameworkElement;
                // If the child's name is set for search
                if (frameworkElement != null && frameworkElement.Name == childName)
                {
                    // if the child's name is of the request name
                    foundChild = child;
                    break;
                }
            }
            else
            {
                // child element found.
                foundChild = child;
                break;
            }
        }

        return foundChild;
    }

    /// <summary>
    /// Finds all children of a given item in the visual tree.
    /// </summary>
    /// <param name="parent">A direct parent of the queried item.</param>
    /// <typeparam name="T">The type of the queried item.</typeparam>
    /// <returns>The all parent items that matches the submitted type parameter. 
    /// If not matching item can be found, 
    /// a null parent is being returned.</returns>
    public static List<T>? FindVisualChildren<T>(this DependencyObject? parent)
        where T : DependencyObject
    {    
        // Confirm parent and childName are valid. 
        if (parent == null) return null;

        List<T> foundChildren = new List<T>();

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            // If the child is not of the request child type child
            T? childType = child as T;
            if (childType == null)
            {
                // recursively drill down the tree
                var visualChildren = FindVisualChildren<T>(child);
                if (visualChildren != null)
                    foundChildren.AddRange(visualChildren);
            }
            else
            {
                // child element found.
                foundChildren.Add((T)child);
                break;
            }
        }

        return foundChildren;
    }

    public static T? FindLogicalParent<T>(this DependencyObject child)
        where T : DependencyObject
    {
        //get parent item
        DependencyObject parentObject = LogicalTreeHelper.GetParent(child);

        //we've reached the end of the tree
        if (parentObject == null) return null;

        //check if the parent matches the type we're looking for
        T? parent = parentObject as T;
        if (parent != null)
            return parent;
        else
            return FindLogicalParent<T>(parentObject);
    }

    public static T? FindVisualParent<T>(this DependencyObject child)
        where T : DependencyObject
    {
        DependencyObject? parentObject = VisualTreeHelper.GetParent(child);

        if (parentObject == null) return null;

        T? parent = parentObject as T;
        if (parent != null)
            return parent;
        return FindVisualParent<T>(parentObject);
    }
}