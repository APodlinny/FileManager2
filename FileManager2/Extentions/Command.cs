using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace FileManager2.Extentions
{
    public class Command : ICommand
    {
        Func<object, bool> _predicate;
        Action<object> _command;

        public Command(Action<object> command)
        {
            _command = command;
            _predicate = null;
        }

        public Command(Action<object> command, Func<object, bool> predicate)
        {
            _command = command;
            _predicate = predicate;
        }

        public bool CanExecute(object parameter)
        {
            return _predicate == null ? true : _predicate(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _command(parameter);
        }
    }
}
