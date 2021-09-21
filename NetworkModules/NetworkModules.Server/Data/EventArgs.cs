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
        public ConnectionViewModel Client { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public string InformationTimeStamp { get; set; }

        /// <summary>
        /// Data received from the client
        /// </summary>
        public string Data           { get; set; }
    }

    /// <summary>
    /// EventArgs that carries information about a download
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class DownloadEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the download.
        /// </summary>
        public DownloadItemViewModel Download { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public string InformationTimeStamp { get; set; }

        /// <summary>
        /// From which client the the download comes from
        /// </summary>
        /// <value>
        /// The client.
        /// </value>
        public ConnectionViewModel Client { get; set; }
    }
}
