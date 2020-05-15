namespace NetworkModules.Server
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.ComponentModel;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    #endregion

    /// <summary>
    /// Model of the Client
    /// </summary>
    public class Connection : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        /// <summary>
        /// Client ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The service that the client uses
        /// </summary>
        public ClientServices Service { get; set; }

        /// <summary>
        /// Connection timestring
        /// </summary>
        public string ConnenctedTime { get; set; } = DateTime.Now.ToString();

        /// <summary>
        /// The connection
        /// </summary>
        public Socket ClientConnection { get; internal set; }

        /// <summary>
        /// Reads the remote ip from the socket
        /// </summary>
        public string ClientConnectionString => ClientConnection.RemoteEndPoint.ToString();

        /// <summary>
        /// This client's global data buffer
        /// </summary>
        public byte[] DataBuffer { get; set; }

        /// <summary>
        /// Size of the data that is currently been received
        /// </summary>
        public long CurrentDataSize { get; set; }

        /// <summary>
        /// The currently decrypted data
        /// </summary>
        public string CurrentDataString { get; set; }

        /// <summary>
        /// This tells the server what this client can
        /// have access to and can do on the server,
        /// can be manually set
        /// </summary>
        public byte AutorisationLevel { get; set; }

        /// <summary>
        /// Has the client verified itself, all connections
        /// should be verified, all active connections should
        /// be verified, if a client fails to veify upon connecting,
        /// it will be disconnected
        /// </summary>
        public bool Verified { get; set; } = false;

        /// <summary>
        /// RSA public key that will be used when sending data to this client
        /// (this key will be provided by the client)
        /// </summary>
        public RSACryptoServiceProvider Encryptor { get; set; }
        /// <summary>
        /// The servers private key
        /// </summary>
        public RSACryptoServiceProvider Decryptor { get; set; }
    }
}
