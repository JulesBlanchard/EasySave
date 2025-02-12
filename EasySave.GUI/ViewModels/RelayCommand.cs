using System;
using System.Windows.Input;

namespace EasySave.GUI
{
    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;
        public event EventHandler CanExecuteChanged;
        
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter) => canExecute == null || canExecute();
        
        public void Execute(object parameter) => execute();
        
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Predicate<T> canExecute;
        public event EventHandler CanExecuteChanged;
        
        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
        
        public bool CanExecute(object parameter)
        {
            if (canExecute == null)
                return true;
            if (parameter == null && typeof(T).IsValueType)
                return canExecute(default);
            return canExecute((T)parameter);
        }
        
        public void Execute(object parameter) => execute((T)parameter);
        
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}