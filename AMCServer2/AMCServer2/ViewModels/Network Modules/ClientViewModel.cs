namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    #endregion

    /// <summary>
    /// Model of the Client
    /// </summary>
    public class ClientViewModel : BaseViewModel
    {
        /// <summary>
        /// Client ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The service that the client uses
        /// </summary>
        public Services Service { get; set; }

        /// <summary>
        /// Connection timestring
        /// </summary>
        public string ConnenctedTime { get; set; } = DateTime.Now.ToString();

        /// <summary>
        /// The connection
        /// </summary>
        public Socket ClientConnection { get; internal set; }

        /// <summary>
        /// This tells the server what this client can
        /// and cannot do
        /// 
        /// ==========================================
        /// 0- Communication only
        /// 1- Access data on the server
        /// ==========================================
        /// 
        /// </summary>
        public byte AutorisationLevel { get; set; }

        /// <summary>
        /// Has the client verified itself
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
