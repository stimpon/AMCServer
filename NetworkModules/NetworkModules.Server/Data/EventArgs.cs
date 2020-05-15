namespace NetworkModules.Server
{
    using System;

    /// <summary>
    /// EventArgs that store data about client and received data
    /// </summary>
    public class ClientInformationEventArgs : EventArgs
    {
        /// <summary>
        /// The client
        /// </summary>
        public Connection Client { get; set; }

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
