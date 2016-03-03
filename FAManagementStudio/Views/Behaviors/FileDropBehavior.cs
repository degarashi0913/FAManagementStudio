using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace FAManagementStudio.Views.Behaviors
{
    class FileDropBehavior : Behavior<ComboBox>
    {
        public FileDropBehavior() { }
        public ICommand DropedCommand
        {
            get { return (ICommand)GetValue(DropedCommandProperty); }
            set { SetValue(DropedCommandProperty, value); }
        }
        public static readonly DependencyProperty DropedCommandProperty = DependencyProperty.Register(nameof(DropedCommand), typeof(ICommand), typeof(FileDropBehavior), new PropertyMetadata(null));

        private void OnDrop(object sender, DragEventArgs e)
        {
            //先頭だけ
            var filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            DropedCommand?.Execute(filePath);
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
