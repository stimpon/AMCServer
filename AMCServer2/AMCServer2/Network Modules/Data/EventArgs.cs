﻿namespace AMCServer2
{
    using System;
    using System.Net.Sockets;

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
        public Responses MessageType { get; set; }
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

    /// <summary>
    /// EventArgs that carries information about a download
    /// </summary>
    public class FileDownloadInformationEventArgs : EventArgs
    {
        /// <summary>
        /// Name of the file
        /// </summary>
        public string FileName      { get; set; }

        /// <summary>
        /// Size of the file
        /// </summary>
        public long FileSize        { get; set; }

        /// <summary>
        /// Current progress (Actual size of the file)
        /// </summary>
        public long ActualFileSize  { get; set; }
    }
}
