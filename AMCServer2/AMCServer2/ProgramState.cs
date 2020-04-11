namespace AMCServer2
{
    /// <summary>
    /// Class that keeps track of the program state
    /// </summary>
    public static class ProgramState
    {
        /// <summary>
        /// This bool is needed for design time and runtime stuff,
        /// some stuff should not execute while the program is not
        /// running.
        /// </summary>
        public static bool IsRunning { get; set; } = false;

    }
}
