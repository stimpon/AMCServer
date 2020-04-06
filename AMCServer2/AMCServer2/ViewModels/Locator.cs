namespace AMCServer2
{
    /// <summary>
    /// Class for locating ViewModels
    /// </summary>
    public class Locator
    {

        /// <summary>
        /// Single instace of the ViewModel Locator
        /// </summary>
        public static Locator Instance { get; private set; } = new Locator();


        #region ViewModelGeters

        /// <summary>
        /// Gets the ApplicationViewModel from the Kernel
        /// </summary>
        public static ApplicationViewModel ApplicationViewModel => IoC.Container.Get<ApplicationViewModel>();

        /// <summary>
        /// Gets the ServerViewModel from the Kernel
        /// </summary>
        public static ServerViewModel ServerViewModel => IoC.Container.Get<ServerViewModel>();

        #endregion

    }
}
