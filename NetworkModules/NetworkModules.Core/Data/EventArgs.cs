namespace NetworkModules.Core
{
    using System;

    /// <summary>
    /// EventArgs that carries information that the client generated
    /// </summary>
    public class InformationEventArgs : EventArgs
    {
        /// <summary>
        /// The message
        /// </summary>
        public IMessage Message { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public string InformationTimeStamp { get; set; }

        /// <summary>
        /// The type of the message
        /// </summary>
        public Responses MessageType { get; set; }
    }
}
