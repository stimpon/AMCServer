namespace AMCServer2.IoC
{
    /// <summary>
    /// Reqiored nemespaces
    /// </summary>
    #region Namespaces
    using Ninject;
    using Ninject.Components;
    #endregion

    /// <summary>
    /// This is the IoC Container
    /// </summary>
    public static class DependencyInjectionCore
    {
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
            Kernel = new StandardKernel();

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
        public static T GetSingleton<T>() => Kernel.Get<T>();
    }
}
