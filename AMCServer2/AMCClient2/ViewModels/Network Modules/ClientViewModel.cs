namespace AMCClient2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    #endregion

    /// <summary>
    /// Client backend ViewModel
    /// </summary>
    public class ClientViewModel : BaseViewModel
    {

        /// <summary>
        /// All of the class's publiv properties
        /// </summary>
        #region Public Properties

        /// <summary>
        /// Listening port for the server
        /// </summary>
        public int ServerPort           { get; private set; }

        /// <summary>
        /// The servers IP Address
        /// </summary>
        public IPAddress ServerIP       { get; private set; }

        /// <summary>
        /// The server socket
        /// </summary>
        public Socket ServerConnection  { get; private set; }

        /// <summary>
        /// Servers packet buffersize
        /// </summary>
        public int GlobalBufferSize     { get; private set; }

        /// <summary>
        /// Current server staus
        /// </summary>
        public ClientStates ClientState { get; private set; }

        /// <summary>
        /// New information event
        /// </summary>
        public EventHandler<InformationEventArgs> ClientInformation;

        #endregion

        #region Private Members

        /// <summary>
        /// The server buffer
        /// </summary>
        private byte[] GlobalBuffer;

        /// <summary>
        /// This programs private key
        /// </summary>
        private RSACryptoServiceProvider Encryptor;

        /// <summary>
        /// Servers public key
        /// </summary>
        private RSACryptoServiceProvider Decryptor;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClientViewModel(int port, IPAddress server_ip)
        {
            /* Port cant be negative but the IPEndpoint Class does
             * not accespt uint's in the parameter
             */
            if (port <= 0)
                throw new InvalidValueException(nameof(port), port);

            // Set the server port
            ServerPort = port;
            ServerIP = server_ip;

            // First initialize
            Initialize();
        }


        #region Functions

        /// <summary>
        /// Initialize the server
        /// </summary>
        public void Initialize()
        {
            // Setup server buffer
            GlobalBufferSize = 1024;
            GlobalBuffer = new byte[GlobalBufferSize];

            Decryptor = new RSACryptoServiceProvider(2048);
            Encryptor = new RSACryptoServiceProvider(2048);

            // Set serverstate to offline
            ClientState = ClientStates.Disconnected;
        }

        /// <summary>
        /// Connect to the server
        /// </summary>
        public void Connect()
        {
            if(ClientState == ClientStates.Connected)
            {
                OnClientInformation("You are already connected to the server", InformationTypes.Warning);
                return;
            }

            // Set the client state
            ClientState = ClientStates.Connecting;

            // Create the socket
            ServerConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Begin connect to the server
            ServerConnection.BeginConnect(new IPEndPoint(ServerIP, ServerPort), new AsyncCallback(ConnectCallback), null);

        }

        /// <summary>
        /// Disconnect from the server
        /// </summary>
        public void Disconnect()
        {

        }

        /// <summary>
        /// Send data to the server
        /// </summary>
        /// <param name="Message"></param>
        public void Send(string Message)
        {
            // Encrypt the data and send it to the server
            ServerConnection.Send(Encryptor.Encrypt(Encoding.Default.GetBytes(Message), true));
        }

        /// <summary>
        /// Event callback
        /// </summary>
        /// <param name="Data"></param>
        protected virtual void OnClientInformation(string Information, InformationTypes type)
        {
            // Check so the event is not null
            if (ClientInformation != null)
                ClientInformation(this, new InformationEventArgs() { Information = Information, MessageType = type });
        }

        #endregion


        #region Socket Callbacks

        /// <summary>
        /// Connect callback
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // End connect
                ServerConnection.EndConnect(ar);

                ServerConnection.Send(Encoding.Default.GetBytes("[VF]"));

                // Receive Server's public key
                { // >>
                    byte[] PK = new byte[500];
                    int len = ServerConnection.Receive(PK);

                    // Resize to the correct size before importing
                    Array.Resize(ref PK, len);
                    Encryptor.ImportRSAPublicKey(PK, out int l);
                } // <<

                // Send RSA key
                ServerConnection.Send(Decryptor.ExportRSAPublicKey());

                OnClientInformation($"Connection to the server was established", InformationTypes.ActionSuccessful);

                // Change the state
                ClientState = ClientStates.Connected;
            }
            catch (Exception ex)
            {
                // Call the event
                OnClientInformation($"Connection failed, error message: {ex.Message}", InformationTypes.Error);

                // Change back the state
                ClientState = ClientStates.Disconnected;
            }

        }

        #endregion
    }
}
