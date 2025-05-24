using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FAManagementStudio.Views.Behaviors;

class TreeViewItemDobleClickBehavior : Behavior<TreeView>
{
    public TreeViewItemDobleClickBehavior() { }
    public ICommand ClickCommand
    {
        get { return (ICommand)GetValue(ClickCommandProperty); }
        set { SetValue(ClickCommandProperty, value); }
    }
    public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(TreeViewItemDobleClickBehavior), new PropertyMetadata(null));


    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseDoubleClick += AssociatedObject_MouseDoubleClick;
    }

    private void AssociatedObject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        static TreeViewItem? FindTreeViewItem(DependencyObject child)
        {
            while (child != null)
            {
                if (child is TreeViewItem item) return item;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        if (e.OriginalSource is DependencyObject source && FindTreeViewItem(source) is TreeViewItem item)
        {
            ClickCommand?.Execute(item.DataContext);
        }
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseDoubleClick -= AssociatedObject_MouseDoubleClick;
        base.OnDetaching();
    }
}
