using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace FAManagementStudio.Views.Behaviors
{
    class TreeViewItemDobleClickBehavior : Behavior<TreeView>
    {
        public TreeViewItemDobleClickBehavior() { }
        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }
        public static readonly DependencyProperty ClickCommandProperty = DependencyProperty.Register(nameof(ClickCommand), typeof(ICommand), typeof(TreeViewItemDobleClickBehavior), new PropertyMetadata(null));

        private object _selectedValue;

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.MouseDoubleClick += AssociatedObject_MouseDoubleClick;
            this.AssociatedObject.SelectedItemChanged += AssociatedObject_SelectedItemChanged;
        }

        private void AssociatedObject_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedValue = e.NewValue;
        }

        private void AssociatedObject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ClickCommand?.Execute(_selectedValue);
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.SelectedItemChanged -= AssociatedObject_SelectedItemChanged;
            this.AssociatedObject.MouseDoubleClick -= AssociatedObject_MouseDoubleClick;
            base.OnDetaching();
        }
    }
}
