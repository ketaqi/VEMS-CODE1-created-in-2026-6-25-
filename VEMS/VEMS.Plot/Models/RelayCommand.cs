using System;
using System.Windows.Input;

namespace VEMS.Plot
{
    public class RelayCommand : ICommand
    {
        private readonly Action action;

        public RelayCommand(Action action)
        {
            this.action = action;
        }

        public event EventHandler? CanExecuteChanged;

        public void Execute(object? parameter)
        {
            action();
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }


    }
}
