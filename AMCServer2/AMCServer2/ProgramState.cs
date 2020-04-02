namespace AMCServer2
{
    /// <summary>
    /// Class that contains everyting about the state
    /// of the program
    /// </summary>
    public static class ProgramState
    {
        /// <summary>
        /// Program state bool
        /// </summary>
        public static bool Running { get; set; }

        /// <summary>
        /// Single instance of the server backend
        /// </summary>
        public static Server ServerBackend { get; set; }

    }
}
