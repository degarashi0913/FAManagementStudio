using FAManagementStudio.Common;
using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using FAManagementStudio.ViewModels.Commons;

namespace FAManagementStudio.Views.Behaviors;

public class ShowWindowAction : TriggerAction<Window>
{
    public Type WindowType
    {
        get => (Type)GetValue(WindowTypeProperty);
        set { SetValue(WindowTypeProperty, value); }
    }
    public Type ViewModelType
    {
        get => (Type)GetValue(ViewModelTypeProperty);
        set { SetValue(ViewModelTypeProperty, value); }
    }
    public bool IsDialog
    {
        get => (bool)GetValue(IsDialogProperty);
        set { SetValue(IsDialogProperty, value); }
    }
    public static readonly DependencyProperty WindowTypeProperty = DependencyProperty.Register(nameof(WindowType), typeof(Type), typeof(ShowWindowAction), new PropertyMetadata(null));

    public static readonly DependencyProperty ViewModelTypeProperty = DependencyProperty.Register(nameof(ViewModelType), typeof(Type), typeof(ShowWindowAction), new PropertyMetadata(null));
    public static readonly DependencyProperty IsDialogProperty = DependencyProperty.Register(nameof(IsDialog), typeof(bool), typeof(ShowWindowAction), new PropertyMetadata(null));

    protected override void Invoke(object parameter)
    {
        if (WindowType == null) return;

        if (Activator.CreateInstance(WindowType) is not Window window) return;
        window.Owner = AssociatedObject;

        var viewModel = ((parameter as MessageBase)?.Sender as ViewModelBase);
        if (viewModel != null) window.DataContext = viewModel;

        if (IsDialog)
        {
            window.ShowDialog();
        }
        else
        {
            window.Show();
        }
    }
}

