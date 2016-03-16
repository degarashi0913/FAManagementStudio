using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace FAManagementStudio.Views.Behaviors
{
    class ListBoxDropBehavior : Behavior<ListBox>
    {
        public ListBoxDropBehavior() { }
        public ICommand DropedCommand
        {
            get { return (ICommand)GetValue(DropedCommandProperty); }
            set { SetValue(DropedCommandProperty, value); }
        }
        public static readonly DependencyProperty DropedCommandProperty = DependencyProperty.Register(nameof(DropedCommand), typeof(ICommand), typeof(ListBoxDropBehavior), new PropertyMetadata(null));

        private void OnDrop(object sender, DragEventArgs e)
        {
            var filePaths = ((string[])e.Data.GetData(DataFormats.FileDrop));
            foreach (var path in filePaths)
            {
                DropedCommand?.Execute(path);
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
