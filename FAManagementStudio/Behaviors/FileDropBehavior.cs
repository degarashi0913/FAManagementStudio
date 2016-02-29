using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace FAManagementStudio.Behavior
{
    class FileDropBehavior : Behavior<ComboBox>
    {
        public ICommand DropedCommand
        {
            get { return (ICommand)GetValue(DropedCommandProperty); }
            set { SetValue(DropedCommandProperty, value); }
        }
        public DependencyProperty DropedCommandProperty { get; } = DependencyProperty.Register("DropedCommand", typeof(ICommand), typeof(FileDropBehavior), new PropertyMetadata(null));

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
            //this.AssociatedObject.AllowDrop = true;
            //var textbox = (TextBox)this.AssociatedObject.Template.FindName("PART_EditableTextBox", this.AssociatedObject);
            //textbox.AllowDrop = true;

        }
        protected override void OnDetaching()
        {
            this.AssociatedObject.Drop -= OnDrop;
            base.OnDetaching();
        }
    }
}
