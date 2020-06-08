namespace NetworkModules.Core
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System.Threading;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using PropertyChanged;
    #endregion

    /// <summary>
    /// A thread safe implementation of ObservableCollection because WPF is stupid.
    /// this class is not bound to wpf, so this class should work cross platform
    /// instead of invoking the Dispatcher when updating the collection from another
    /// thread.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DoNotNotify]
    public class ThreadSafeObservableCollection<T> : 
                 ObservableCollection<T> {

        /// <summary>
        /// The thead that the collection was created on
        /// </summary>
        public SynchronizationContext _sync = SynchronizationContext.Current;

        #region Constructors

        /// <summary>
        /// Empty constructor
        /// </summary>
        public ThreadSafeObservableCollection() { }

        /// <summary>
        /// Construcor that takes an IEnumerable
        /// </summary>
        /// <param name="_enumerable"></param>
        public ThreadSafeObservableCollection(IEnumerable<T> _enumerable)
            : base(_enumerable) {
        }

        #endregion


        #region Functions

        /// <summary>
        /// Gets called when the collecion has changed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            /* Check if the collection is being chaned
             * from the thead that the collection was
             * created on, if not, then send the action
             * to that thread instead.
             */

            if (IsInCorrectThread) RaiseCollectionChanged(e);
            else _sync.Send(RaiseCollectionChanged, e);
            
        }

        /// <summary>
        /// When a property has changed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            /* Check if the collection is being chaned
             * from the thead that the collection was
             * created on, if not, then send the action
             * to that thread instead.
             */

            if (IsInCorrectThread) RaisePropertyChanged(e);
            else _sync.Send(RaisePropertyChanged, e);
        }

        #region Events

        private void RaisePropertyChanged(object parameter) =>
            base.OnPropertyChanged((PropertyChangedEventArgs)parameter);
        private void RaiseCollectionChanged(object parameter) =>
            base.OnCollectionChanged((NotifyCollectionChangedEventArgs)parameter);

        #endregion

        /// <summary>
        /// Check if in the thread the collection was created on
        /// </summary>
        private bool IsInCorrectThread => 
            (SynchronizationContext.Current == _sync) 
                ? true: false;

        #endregion
    }
}
