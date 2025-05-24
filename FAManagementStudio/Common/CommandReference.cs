using System;
using System.Windows;
using System.Windows.Input;

namespace FAManagementStudio.Common;

/// <summary>
/// This class facilitates associating a key binding in XAML markup to a command
/// defined in a View Model by exposing a Command dependency property.
/// The class derives from Freezable to work around a limitation in WPF when data-binding from XAML.
/// </summary>
public class CommandReference : Freezable, ICommand
{
    /// <summary>
    /// Default constructor for design time data.
    /// </summary>
#pragma warning disable CS8618
    public CommandReference() { }
#pragma warning restore CS8618 

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(CommandReference), new PropertyMetadata(new PropertyChangedCallback(OnCommandChanged)));

    public ICommand Command
    {
        get { return (ICommand)GetValue(CommandProperty); }
        set { SetValue(CommandProperty, value); }
    }

    #region ICommand Members

    public bool CanExecute(object? parameter)
        => Command?.CanExecute(parameter) ?? false;

    public void Execute(object? parameter)
        => Command.Execute(parameter);

    public event EventHandler? CanExecuteChanged;

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CommandReference commandReference) return;

        if (e.OldValue is ICommand oldCommand)
        {
            oldCommand.CanExecuteChanged -= commandReference.CanExecuteChanged;
        }
        if (e.NewValue is ICommand newCommand)
        {
            newCommand.CanExecuteChanged += commandReference.CanExecuteChanged;
        }
    }

    #endregion

    #region Freezable

    protected override Freezable CreateInstanceCore()
    {
        throw new NotImplementedException();
    }

    #endregion
}
