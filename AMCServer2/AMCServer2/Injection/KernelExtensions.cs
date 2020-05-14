namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using Ninject;
    using NetworkModulesServer;
    #endregion

    /// <summary>
    /// Contains extension methods for <see cref="IKernel", <seealso cref="StandardKernel"/>/>
    /// </summary>
    public static class KernelExtensions
    {
        /// <summary>
        /// Constructs the specified instance.
        /// </summary>
        /// <param name="K">The k.</param>
        /// <returns></returns>
        public static IK Construct<IK> (this IK K) where IK: IKernel
        {
            // Check if K is null
            if (K is null)
                throw new System.ArgumentNullException(nameof(K));

            #region Constant bindings

            // ApplicationViewModel
            K.Bind<ApplicationViewModel>().ToConstant(new ApplicationViewModel());

            //                                                           - Read all properties from the config file
            //                                                           - Pass them to the constructor of the ServerViewModel
            K.Bind<ServerHandler>().ToConstant(new ServerHandler(          ConfigFilesProcessor.GetServerPort(),
                                                                           ConfigFilesProcessor.GetServerBacklog(),
                                                                           ConfigFilesProcessor.GetServerBufferSize()));

            #endregion

            // Return the new kernel
            return K;
        }

    }
}
