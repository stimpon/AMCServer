namespace AMCClient2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using Ninject;
    using NetworkModulesClient;
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

            K.Bind<ApplicationViewModel>().ToConstant(new ApplicationViewModel());

            K.Bind<ClientHandler>().ToConstant(new ClientHandler(         ConfigFilesProcessor.GetServerPort(),
                                                                          ConfigFilesProcessor.GetServerIPAddress()));

            #endregion

            // Return the new kernel
            return K;
        }

    }
}
