using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace FAManagementStudio.Views.Behaviors
{
    public class WindowBehavior : Behavior<Window>
    {
        public ICommand LoadedCommand
        {
            get { return (ICommand)GetValue(LoadedCommandProperty); }
            set { SetValue(LoadedCommandProperty, value); }
        }

        public ICommand ClosedCommand
        {
            get { return (ICommand)GetValue(ClosedCommandProperty); }
            set { SetValue(ClosedCommandProperty, value); }
        }

        public static readonly DependencyProperty LoadedCommandProperty = DependencyProperty.Register(nameof(LoadedCommand), typeof(ICommand), typeof(WindowBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty ClosedCommandProperty = DependencyProperty.Register(nameof(ClosedCommand), typeof(ICommand), typeof(WindowBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Closed += AssociatedObject_Closed;
        }

        private void AssociatedObject_Closed(object? sender, EventArgs e)
        {
            ClosedCommand?.Execute(null);
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            LoadedCommand?.Execute(null);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Closed -= AssociatedObject_Closed;
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            base.OnDetaching();
        }
    }
}
