using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Media;

namespace FAManagementStudio.Views.Behaviors;

public class BindableScaleBehavior : Behavior<FrameworkElement>
{
    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }
    public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(nameof(Scale), typeof(double), typeof(BindableScaleBehavior), new PropertyMetadata(1.0, OnScaleChanged));


    private static void OnScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var behavior = (BindableScaleBehavior)d;
        behavior.ApplyScale();
    }

    private void ApplyScale()
    {
        if (AssociatedObject == null) return;

        if (AssociatedObject.LayoutTransform is ScaleTransform scale)
        {
            scale.ScaleX = Scale;
            scale.ScaleY = Scale;
        }
        else
        {
            AssociatedObject.LayoutTransform = new ScaleTransform(Scale, Scale);
        }
    }
}
