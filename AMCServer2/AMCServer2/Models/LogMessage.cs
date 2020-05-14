namespace AMCServer2
{
    using NetworkModules.Core;

    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    #endregion

    /// <summary>
    /// Item that will be displayed in the server log
    /// </summary>
    public class LogMessage : ILogMessage
    {
        /// <summary>
        /// Log time
        /// </summary>
        public string EventTime        { get; set; }

        /// <summary>
        /// This is the content of the item
        /// </summary>
        public string Content          { get; set; }

        /// <summary>
        /// Should the timestamp be displayed in the server log?
        /// </summary>
        public bool ShowTime           { get; set; }

        /// <summary>
        /// Type of log
        /// </summary>
        public Responses Type   { get; set; }
    }
}
