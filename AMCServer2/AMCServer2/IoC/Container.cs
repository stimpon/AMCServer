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

            // ViewModel binding
            Kernel.Bind<ApplicationViewModel>().ToConstant(new ApplicationViewModel());
            Kernel.Bind<ServerViewModel>().ToConstant(new ServerViewModel(400, 3));
        }

        /// <summary>
        /// Returns the object from the kernel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() => Kernel.Get<T>();
    }
}
