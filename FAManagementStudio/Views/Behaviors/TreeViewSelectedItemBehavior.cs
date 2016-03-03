using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace FAManagementStudio.Views.Behaviors
{
    class TreeViewSelectedItemBehavior : Behavior<TreeView>
    {
        public TreeViewSelectedItemBehavior() { }
        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(TreeViewSelectedItemBehavior), new UIPropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (e.NewValue as TreeViewItem)?.SetValue(TreeViewItem.IsSelectedProperty, true);
        }
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectedItemChanged += SelectedItemChanged;
        }
        protected override void OnDetaching()
        {
            this.AssociatedObject.SelectedItemChanged -= SelectedItemChanged;
            base.OnDetaching();
        }
        private void SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            this.SelectedItem = e.NewValue;
        }
    }
}
