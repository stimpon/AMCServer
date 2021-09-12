namespace NetworkModules.Server
{
    // Required namespaces
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using NetworkModules.Core;

    /// <summary>
    /// Client ViewModel
    /// This class can be extended for further functionality
    /// </summary>
    public partial class ConnectionViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        #region Client info

        /// <summary>
        /// Client ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Connection timestring
        /// </summary>
        public string ConnenctedTime { get; set; } = DateTime.Now.ToString();

        /// <summary>
        /// The socket for this connection
        /// </summary>
        public Socket ClientConnection { get; internal set; }

        /// <summary>
        /// Has the client verified itself, all connections
        /// should be verified, all active connections should
        /// be verified, if a client fails to veify upon connecting,
        /// it will be disconnected
        /// </summary>
        public bool Verified { get; set; } = false;

        /// <summary>
        /// Gets or sets this connection's permissions on the server
        /// </summary>
        public Permissions ServerPermissions { get; set; }

        #endregion

        #region Client connection

        /// <summary>
        /// Reads the remote ip + port from the socket
        /// </summary>
        public string ClientConnectionString { get => ClientConnection.RemoteEndPoint.ToString(); }

        /// <summary>
        /// Gets this clients IP address
        /// </summary>
        public string IP { get => ClientConnectionString.Split(':')[0]; }

        /// <summary>
        /// This client's global data buffer
        /// </summary>
        /// 
        public byte[] DataBuffer { get; set; }
        /// <summary>
        /// Gets or sets the byte queue for this client (The bytes that are being handled at the moment)
        /// </summary>
        public List<byte> ByteQueue { get; set; } = new List<byte>();

        /// <summary>
        /// Size of the data that is currently been received
        /// </summary>
        public long CurrentDataSize { get; set; }

        /// <summary>
        /// Size of the data that is currently been received
        /// </summary>
        public byte[] CurrentDataSignature { get; set; }

        /// <summary>
        /// The currently decrypted data
        /// </summary>
        public string CurrentDataString { get; set; }

        #endregion

        #region File transfer properties

        /// <summary>
        /// Gets or sets the file transfer connection.
        /// </summary>
        /// <value>
        /// The file transfer connection.
        /// </value>
        public Socket FTSocket { get; set; }

        /// <summary>
        /// This client's global data buffer
        /// </summary>
        public byte[] DownloadBuffer { get; set; }

        /// <summary>
        /// Gets or sets the byte queue for this client (The bytes that are being handled at the moment)
        /// </summary>
        public List<byte> DownloadByteQueue { get; set; } = new List<byte>();

        #endregion

        #region Cryptography

        /// <summary>
        /// RSA public key that will be used when sending data to this client
        /// (this key will be provided by the client)
        /// </summary>
        public RSACryptoServiceProvider Encryptor { get; set; }
        /// <summary>
        /// The servers private key
        /// </summary>
        public RSACryptoServiceProvider Decryptor { get; set; }

        #endregion
    }
}
