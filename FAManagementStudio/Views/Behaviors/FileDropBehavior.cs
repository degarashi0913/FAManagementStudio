using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace FAManagementStudio.Views.Behaviors
{
    public class FileDropBehavior : Behavior<FrameworkElement>
    {
        public FileDropBehavior() { }
        public ICommand DropedCommand
        {
            get { return (ICommand)GetValue(DropedCommandProperty); }
            set { SetValue(DropedCommandProperty, value); }
        }

        public FilePathFetchMode FetchMode
        {
            get { return (FilePathFetchMode)GetValue(FetchModeProperty); }
            set { SetValue(FetchModeProperty, value); }
        }

        public static readonly DependencyProperty DropedCommandProperty = DependencyProperty.Register(nameof(DropedCommand), typeof(ICommand), typeof(FileDropBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty FetchModeProperty = DependencyProperty.Register(nameof(FetchModeProperty), typeof(FilePathFetchMode), typeof(FileDropBehavior), new PropertyMetadata(FilePathFetchMode.All));

        private void OnDrop(object sender, DragEventArgs e)
        {
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (paths == null) return;
            var count = FetchMode == FilePathFetchMode.Once ? 1 : paths.Length;
            for (var i = 0; i < count; i++)
            {
                DropedCommand?.Execute(paths[i]);
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
    public enum FilePathFetchMode { All, Once }
}
