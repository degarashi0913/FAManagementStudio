using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace FAManagementStudio.Views.Behaviors
{
    public class DynamicTabControlBehavior : Behavior<TabControl>
    {
        public ICommand AddTabCommand
        {
            get => (ICommand)GetValue(AddTabCommandProperty);
            set => SetValue(AddTabCommandProperty, value);
        }

        public static readonly DependencyProperty AddTabCommandProperty = DependencyProperty.Register(nameof(AddTabCommand), typeof(ICommand), typeof(DynamicTabControlBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((AssociatedObject.Items.Count - 1) == AssociatedObject.SelectedIndex)
            {
                AddTabCommand?.Execute(null);
            }
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            base.OnDetaching();
        }
    }
}
