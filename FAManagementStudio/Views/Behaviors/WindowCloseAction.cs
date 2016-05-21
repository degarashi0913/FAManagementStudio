using System;
using System.Windows;
using System.Windows.Interactivity;

namespace FAManagementStudio.Views.Behaviors
{
    public class WindowCloseAction : TriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            Window.GetWindow(AssociatedObject).Close();
        }
    }
}
