using System;
using System.Windows.Input;

namespace FAManagementStudio.Common;

class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action execute) : this(execute, () => true)
    {
    }

    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _execute = execute;
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

class RelayCommand<T>(Action<T> execute, Func<bool> canExecute) : ICommand
{
    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action<T> execute) : this(execute, () => true)
    {
    }

    public bool CanExecute(object? parameter) => canExecute == null || canExecute();

    public void Execute(object? parameter)
    {
        if (parameter is null) return;

        execute((T)parameter);
    }

    public void RaiseCanExecuteChanged()
       => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
