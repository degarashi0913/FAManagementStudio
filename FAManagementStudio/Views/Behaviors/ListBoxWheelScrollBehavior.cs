using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace FAManagementStudio.Views.Behaviors
{
    //Kill the Mouse Wheel Events for the Parent ScrllView 
    class ListBoxWheelScrollBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseWheel += PreviewMouseWheel;
        }
        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseWheel -= PreviewMouseWheel;
            base.OnDetaching();
        }

        private void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            ((UIElement)AssociatedObject.Parent).RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) { RoutedEvent = UIElement.MouseWheelEvent });
        }
    }
}
