namespace AMCClient2
{
    /// <summary>
    /// Class for locating ViewModels in the kernel
    /// </summary>
    public class Locator
    {

        /// <summary>
        /// Single instace of the ViewModel Locator so that it can be accessed in XAML
        /// </summary>
        public static Locator Instance { get; private set; } = new Locator();


        #region Kernel ViewModels

        /// <summary>
        /// Gets the ApplicationViewModel from the Kernel
        /// </summary>
        public static ApplicationViewModel ApplicationViewModel 
            => 
            IoC.Container.Get<ApplicationViewModel>();

        /// <summary>
        /// Gets the ServerViewModel from the Kernel
        /// </summary>
        public static ClientViewModel ServerViewModel 
            =>
            IoC.Container.Get<ClientViewModel>();

        #endregion

    }
}
