using Ninject;

namespace AMCServer2.IoC
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
            /* This function should run on startup and it
             * will bind all neccesery ViewModel's to the
             * kernel
             */

            // This is the underlaying application ViewModel
            Kernel.Bind<ApplicationViewModel>().ToConstant(new ApplicationViewModel());

            //                                                           - Read all properties from the config file
            //                                                           - Pass them to the constructor of the ServerViewModel
            Kernel.Bind<ServerViewModel>().ToConstant(new ServerViewModel( ConfigFilesProcessor.GetServerPort(),
                                                                           ConfigFilesProcessor.GetServerBacklog(),
                                                                           ConfigFilesProcessor.GetServerBufferSize()));
        }

        /// <summary>
        /// Returns the object from the kernel
        /// </summary>
        /// <typeparam name="T">Type to get from the kernel</typeparam>
        /// <returns>returns the boudn item</returns>
        public static T Get<T>() => Kernel.Get<T>();
    }
}
