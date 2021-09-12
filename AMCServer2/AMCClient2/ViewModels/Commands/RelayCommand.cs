namespace AMCClient2
{
    // Required namespaces >>
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Universal command
    /// </summary>
    public class RelayCommand : ICommand
    {
        /// <summary>
        /// Fires when CanExecute is changed
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add    { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Command action, is set in the constructor
        /// </summary>
        private Action<object> _action;
        /// <summary>
        /// Command predicate, is set in the constructor
        /// </summary>
        private Predicate<object> _canExecute;

        /// <summary>
        /// Class constructors
        /// </summary>
        #region Constructors
        /// <summary>
        /// Default constructor with Action and Predicate
        /// </summary>
        /// <param name="action"></param>
        /// <param name="predicate"></param>
        public RelayCommand(Action<object> action, Predicate<object> predicate)
        {
            this._action = action;        // Set the command action
            this._canExecute = predicate; // Set the command predicate
        }

        /// <summary>
        /// Default constructor with no predicate (always true)
        /// </summary>
        /// <param name="action"></param>
        public RelayCommand(Action<object> action)
        {
            this._action = action;        // Set the command action
            this._canExecute = (o) => true;
        }
        #endregion

        /// <summary>
        /// Returns true if command can be executed
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter) => 
            
            _canExecute.Invoke(parameter);

        /// <summary>
        /// Executes the command action
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter) => 
            
            _action.Invoke(parameter);
    }
}
