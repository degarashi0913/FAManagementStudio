using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace FAManagementStudio.Views.Behaviors
{
    public class GetFocusWhenMovedTabKeyBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.IsKeyboardFocusWithinChanged += AssociatedObject_IsKeyboardFocusWithinChanged;
        }

        private void AssociatedObject_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var listbox = AssociatedObject as ListBox;
            if (listbox == null) return;
            if (listbox.IsKeyboardFocusWithin && listbox.SelectedItem == null)
            {
                listbox.SelectedIndex = 0;
            }
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.IsKeyboardFocusWithinChanged -= AssociatedObject_IsKeyboardFocusWithinChanged;
            base.OnDetaching();
        }
    }
}
