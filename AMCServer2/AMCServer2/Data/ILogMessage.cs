namespace AMCServer2
{
    /// <summary>
    /// Interface for the all items that will be displayed
    /// in the server log
    /// </summary>
    public interface ILogMessage
    {
        /// <summary>
        /// Log time
        /// </summary>
        public string EventTime { get; set; }

        /// <summary>
        /// This is the content of the item
        /// </summary>
        public string Content { get; set; }
    }
}
