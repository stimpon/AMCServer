namespace AMCServer2
{
    // Required namespaces >>
    using System;
    using System.Windows.Input;

    /// <summary>
    /// RelayCommand that does not contain any parameters
    /// </summary>
    public class RelayCommandNoParameters : ICommand
    {
        // CanExecute criteria
        private Predicate<object> Criteria;
        // Executing function(s)
        private Action _Action;

        /// <summary>
        /// Fires when CanExectute has changed
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add    { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested += value; }
        }

        /// <summary>
        /// Class constructors
        /// </summary>
        #region Constructors
        /// <summary>
        /// Default constructor with Action and Predicate
        /// </summary>
        /// <param name="action"></param>
        /// <param name="criteria"></param>
        public RelayCommandNoParameters(Action action, Predicate<object> criteria)
        {
            _Action = action;
            Criteria = criteria;
        }

        /// <summary>
        /// Constructor with no Predicate (always true)
        /// </summary>
        /// <param name="action"></param>
        public RelayCommandNoParameters(Action action)
        {
            _Action = action;
            Criteria = (o) => true;
        }
        #endregion

        /// <summary>
        /// Can the command execute
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter) => 
            
            Criteria.Invoke(parameter);

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter) =>
            
            _Action.Invoke();
    }
}
