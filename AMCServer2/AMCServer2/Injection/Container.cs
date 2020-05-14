namespace AMCServer2
{
    /// <summary>
    /// Reqiored nemespaces
    /// </summary>
    #region Namespaces
    using Ninject;
    using NetworkModulesServer;
    #endregion

    /// <summary>
    /// This is the IoC Container
    /// </summary>
    public class Container
    {
        public static Container Instance => new Container();

        /// <summary>
        /// Application kernel
        /// </summary>
        public static IKernel Kernel { get; private set; }

        /// <summary>
        /// Method that setups the IoC Container
        /// </summary>
        public static void Configure()
        {
            // Create the application kernel
            Kernel = new StandardKernel().Construct();
        }

        /// <summary>
        /// Returns the object from the kernel
        /// </summary>
        /// <typeparam name="T">Type to get from the kernel</typeparam>
        /// <returns>returns the boudn item</returns>
        public static T GetSingleton<T>() => Kernel.Get<T>();


        /// <summary>
        /// Gets the <see cref="ApplicationViewModel"/> From the container
        /// </summary>
        /// <returns></returns>
        public static ApplicationViewModel ApplicationViewModel
        =>
        Kernel.Get<ApplicationViewModel>();

        /// <summary>
        /// Gets the <see cref="ServerHandler"/> From the container
        /// </summary>
        /// <returns></returns>
        public static ServerHandler ServerHandler
        =>
        Kernel.Get<ServerHandler>();
    }
}
