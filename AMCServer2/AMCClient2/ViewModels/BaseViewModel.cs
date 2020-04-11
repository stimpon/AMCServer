namespace AMCClient2
{
    using System.ComponentModel;

    /// <summary>
    /// Base ViewModel for all other ViewModels
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Fires when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        /// <summary>
        /// If the event needs to be fired manually
        /// </summary>
        /// <param name="VM"></param>
        /// <param name="e"></param>
        protected virtual void OnPropertyChanged(object VM, PropertyChangedEventArgs e) => PropertyChanged?.Invoke(VM, e);
    }
}
