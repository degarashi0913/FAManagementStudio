using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace FAManagementStudio.Behaviors
{
    class ListBoxDropBehavior : Behavior<ListBox>
    {
        public ListBoxDropBehavior() { }
        public ICommand ListBoxDropedCommand
        {
            get { return (ICommand)GetValue(TreeViewDropedCommandProperty); }
            set { SetValue(TreeViewDropedCommandProperty, value); }
        }
        public static readonly DependencyProperty TreeViewDropedCommandProperty = DependencyProperty.Register(nameof(ListBoxDropedCommand), typeof(ICommand), typeof(ListBoxDropBehavior), new PropertyMetadata(null));

        private void OnDrop(object sender, DragEventArgs e)
        {
            //先頭だけ
            var filePaths = ((string[])e.Data.GetData(DataFormats.FileDrop));
            foreach (var path in filePaths)
            {
                ListBoxDropedCommand?.Execute(path);
            }
        }
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Drop += OnDrop;
        }
        protected override void OnDetaching()
        {
            this.AssociatedObject.Drop -= OnDrop;
            base.OnDetaching();
        }
    }
}
