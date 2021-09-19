/// <summary>
/// Root namespace
/// </summary>
namespace AMCClient2
{
    #region Required namespaces
    using NetworkModules.Client;
    using Ninject;
    #endregion

    /// <summary>
    /// This is the IoC Container
    /// </summary>
    public class Container
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static Container Instance => new Container(); 

        /// <summary>
        /// Application kernel
        /// </summary>
        public static IKernel Kernel { get; private set; }

        /// <summary>
        /// Method that setups the IoC Container
        /// </summary>
        public static void SetupIoC()
        {
            Kernel = new StandardKernel().Construct();
        }

        /// <summary>
        /// Returns the object from the kernel
        /// </summary>
        /// <typeparam name="T">Type to get from the kernel</typeparam>
        /// <returns>returns the boudn item</returns>
        public static T Get<T>() => Kernel.Get<T>();

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
        public static ClientHandler ClientHandler
        =>
        Kernel.Get<ClientHandler>();
    }
}
