using Ninject;

namespace AMCClient2.IoC
{
    /// <summary>
    /// This is the IoC Container
    /// </summary>
    public static class Container
    {
        /// <summary>
        /// Application kernel
        /// </summary>
        public static IKernel Kernel { get; private set; } = new StandardKernel();

        /// <summary>
        /// Method that setups the IoC Container
        /// </summary>
        public static void SetupIoC()
        {

            // ViewModel binding
            Kernel.Bind<ApplicationViewModel>().ToConstant(new ApplicationViewModel());
            Kernel.Bind<ClientViewModel>().ToConstant(new ClientViewModel(ConfigFilesProcessor.GetServerPort(),
                                                                          ConfigFilesProcessor.GetServerIPAddress()));
        }

        /// <summary>
        /// Returns the object from the kernel
        /// </summary>
        /// <typeparam name="T">Type to get from the kernel</typeparam>
        /// <returns>returns the boudn item</returns>
        public static T Get<T>() => Kernel.Get<T>();
    }
}
