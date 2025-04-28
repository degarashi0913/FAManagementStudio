using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FAManagementStudio.Views.Behaviors;

public class MouseDoubleClickBehavior : Behavior<Control>
{
    public ICommand DoubleClickCommand
    {
        get => (ICommand)GetValue(DoubleClickCommandProperty);
        set { SetValue(DoubleClickCommandProperty, value); }
    }

    public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register(nameof(DoubleClickCommand), typeof(ICommand), typeof(MouseDoubleClickBehavior), new PropertyMetadata(null));

    private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        DoubleClickCommand?.Execute(null);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject.MouseDoubleClick += OnMouseDoubleClick;
    }

    protected override void OnDetaching()
    {
        AssociatedObject.MouseDoubleClick -= OnMouseDoubleClick;
        base.OnDetaching();
    }
}
