namespace AMCClient2
{
    using System;

    /// <summary>
    /// EventArgs that store data
    /// </summary>
    public class InformationEventArgs : EventArgs
    {
        /// <summary>
        /// Data
        /// </summary>
        public string Information           { get; set; }

        /// <summary>
        /// The type of the message
        /// </summary>
        public InformationTypes MessageType { get; set; }
    }
}
