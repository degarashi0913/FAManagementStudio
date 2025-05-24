using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace FAManagementStudio.Views.Behaviors
{
    public class ImageButtonBehavior : Behavior<Button>
    {
        public bool ChangeEnable
        {
            get { return (bool)GetValue(ChangeEnableProperty); }
            set { SetValue(ChangeEnableProperty, value); }
        }
        public static readonly DependencyProperty ChangeEnableProperty = DependencyProperty.Register(nameof(ChangeEnable), typeof(bool), typeof(ImageButtonBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Click += OnClick;
        }
        
        private void OnClick(object sender, RoutedEventArgs e)
        {
            ChangeEnable = !ChangeEnable;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.Drop -= OnClick;
            base.OnDetaching();
        }
    }
}
