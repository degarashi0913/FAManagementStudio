using FAManagementStudio.Common;
using FAManagementStudio.ViewModels;
using System;
using System.Windows;
using System.Windows.Interactivity;

namespace FAManagementStudio.Views.Behaviors
{
    public class ShowWindowAction : TriggerAction<Window>
    {
        public Type WindowType
        {
            get { return (Type)GetValue(WindowTypeProperty); }
            set { SetValue(WindowTypeProperty, value); }
        }
        public Type ViewModelType
        {
            get { return (Type)GetValue(ViewModelTypeProperty); }
            set { SetValue(ViewModelTypeProperty, value); }
        }
        public static readonly DependencyProperty WindowTypeProperty = DependencyProperty.Register(nameof(WindowType), typeof(Type), typeof(ShowWindowAction), new PropertyMetadata(null));

        public static readonly DependencyProperty ViewModelTypeProperty = DependencyProperty.Register(nameof(ViewModelType), typeof(Type), typeof(ShowWindowAction), new PropertyMetadata(null));

        protected override void Invoke(object parameter)
        {
            if (this.WindowType == null) { return; }

            var window = Activator.CreateInstance(this.WindowType) as Window;
            window.Owner = this.AssociatedObject;

            var viewModel = ((parameter as MessageBase)?.Sender as ViewModelBase);
            if (viewModel != null) window.DataContext = viewModel;

            window.ShowDialog();

            viewModel = (ViewModelBase)window.DataContext;
        }
    }
}

