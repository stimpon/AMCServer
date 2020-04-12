namespace AMCServer2
{
    using System;

    /// <summary>
    /// EventArgs that store messages from the server
    /// </summary>
    public class InformationEventArgs : EventArgs
    {
        /// <summary>
        /// Data
        /// </summary>
        public string Information           { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public string InformationTimeStamp  { get; set; }

        /// <summary>
        /// The type of the message
        /// </summary>
        public InformationTypes MessageType { get; set; }
    }

    /// <summary>
    /// EventArgs that store data about client and received data
    /// </summary>
    public class ClientInformationEventArgs : EventArgs
    {
        /// <summary>
        /// The client
        /// </summary>
        public ClientViewModel Client { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public string InformationTimeStamp { get; set; }

        /// <summary>
        /// Data received from the client
        /// </summary>
        public string Data           { get; set; }
    }
}
