namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Security.Cryptography;
    using System.Linq;
    #endregion

    /// <summary>
    /// This is the server backend class
    /// </summary>
    public class ServerViewModel : BaseViewModel
    {
        /// <summary>
        /// All of the public properties
        /// </summary>
        #region Public Properties

        /// <summary>
        /// Listening port for the server
        /// </summary>
        public int ListeningPort { get; private set; }

        /// <summary>
        /// Server backlog
        /// </summary>
        public int ServerBacklog { get; private set; }

        /// <summary>
        /// Server IPEndPoint
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// The server socket
        /// </summary>
        public Socket ServerSocket                     { get; private set; }

        /// <summary>
        /// All of the active connections
        /// </summary>
        public List<ClientViewModel> ActiveConnections { get; private set; }

        /// <summary>
        /// Current server staus
        /// </summary>
        public ServerStates ServerState { get; private set; }

        /// <summary>
        /// Servers packet buffersize
        /// </summary>
        public int ServerBufferSize     { get; private set; }

        /// <summary>
        /// If true, then any sockets that does not provide
        /// handshake after connecting will be dropped
        /// </summary>
        public bool 
            RequireHandShakeMessage     { get; set; }
        /// <summary>
        /// The handshake message that needs to be provided
        /// from the clien when connecting
        /// </summary>
        public string HandShakeString   { get; set; }

        #endregion

        /// <summary>
        /// All of the private members
        /// </summary>
        #region Private Members

        /// <summary>
        /// The server buffer
        /// </summary>
        private byte[] ServerBuffer;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ServerViewModel
            (int port, int backlog) {

            /* Port cant be negative but the IPEndpoint Class does
             * not accespt uint's in the parameter
             */
            if (port <= 0)
                throw new InvalidValueException(nameof(port), port);

            ListeningPort = port;
            ServerBacklog = backlog;

            Initialize(); 
        }

        #region Functions

        /// <summary>
        /// Initialize the server
        /// </summary>
        public void Initialize()
        {
            // Setup server buffer
            ServerBufferSize = 1024;
            ServerBuffer     = new byte[ServerBufferSize];

            EndPoint = new IPEndPoint(IPAddress.Any, ListeningPort);

            // If server should require handshake
            RequireHandShakeMessage = false;
            HandShakeString         = String.Empty;

            // Create empty connections list
            ActiveConnections       = new List<ClientViewModel>();
            
            // Set serverstate to offline
            ServerState = ServerStates.Offline;
        }

        /// <summary>
        /// Start the server
        /// </summary>
        /// <returns>True if successful</returns>
        public void StartServer()
        {
            // Quit if socket has not been set
            if (ServerState == ServerStates.Online) return;

            // Set server state
            ServerState = ServerStates.StartingUp;

            // Create the socket object
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the specified port
            ServerSocket.Bind(EndPoint);
            // Begin listening
            ServerSocket.Listen(ServerBacklog);

            // Begin accepting connections async
            ServerSocket.BeginAccept(new AsyncCallback(ServerAcceptCallback), null);

            // Startup was successful >>
            ServerState = ServerStates.Online;
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Shutdown()
        {
            // Closer all actice connections
            foreach (var Client in ActiveConnections)
                Client.ClientConnection.Close();
            ActiveConnections.Clear();

            // Close the socket
            ServerSocket.Close();

            // Startup complete >>
            ServerState = ServerStates.Offline;
        }


        #endregion


        #region Socket callbacks

        /// <summary>
        /// When a client has connected
        /// </summary>
        /// <param name="ar"></param>
        private void ServerAcceptCallback(IAsyncResult ar)
        {
            // This is the connection that has been made
            Socket s = ServerSocket.EndAccept(ar);

            try
            {
                // Wait for handshake if set to do so
                if (RequireHandShakeMessage) 
                {
                    // Set handshake timeout
                    s.ReceiveTimeout = 1000;
                    s.Receive(new byte[HandShakeString.Length]);              
                }

                // Create the Client obejct
                var ClientConnection = new ClientViewModel()
                {
                    ClientConnection = s,
                    AutorisationLevel = 0
                };

                // Create a unique RSA keypair for this client
                ClientConnection.Encryptor = new RSACryptoServiceProvider(2048);

                // Send server's public key >>
                s.Send(ClientConnection.Encryptor.ExportRSAPublicKey());

                // Set key exchange timeout
                s.ReceiveTimeout = 5000;

                // Receive Clients's public key
                { // >>
                    byte[] PK = new byte[500];
                    int len = s.Receive(PK);

                    // Resize to the correct size before importing
                    Array.Resize(ref PK, len);
                    ClientConnection.Decryptor.ImportRSAPublicKey(PK, out int l);
                } // <<

                // Begin listening to the client
                s.BeginReceive(ServerBuffer, 0, ServerBuffer.Length,
                                                                SocketFlags.None,
                                                                new AsyncCallback(ServerReceiveCallback),
                                                                ClientConnection.ClientConnection);

                // Add the new connection to the list of connections
                ActiveConnections.Add(ClientConnection);
            }
            catch
            {
                // Close the socket if there were any problems
                s.Close();
            }
            finally
            {
                // Begin accepting more connections
                ServerSocket.BeginAccept(new AsyncCallback(ServerAcceptCallback), null);
            }

        }

        /// <summary>
        /// When the server receives data from a client
        /// </summary>
        /// <param name="ar"></param>
        private void ServerReceiveCallback(IAsyncResult ar)
        {
            // This can fail if client disconnects
            try
            {
                // Get the socket
                Socket Client = (Socket)ar.AsyncState;

                // Get the conenction object from the list of active connections
                var ClientVM = ActiveConnections.First(c => c.ClientConnection.Equals(Client));

                // Check the lengt of the sent data
                int Rec = Client.EndReceive(ar);

                // Check if an empty packet was received
                if (Rec > 0)
                {  
                    // Create new buffer and resize it to the correct size
                    byte[] ReceivedBytes = ServerBuffer;
                    Array.Resize(ref ReceivedBytes, Rec);

                    // Decrypt and convert the received bytes to a string
                    string Message = Encoding.Default.GetString(
                                     ClientVM.Decryptor.Decrypt(
                                         ReceivedBytes, true) 
                                     );
                }

                // Begin receiving again
                Client.BeginReceive(ServerBuffer, 0, ServerBuffer.Length,
                                                     SocketFlags.None,
                                                     new AsyncCallback(ServerReceiveCallback),
                                                     Client);
            }
            // Remove the connection
            catch { }
        }

        /// <summary>
        /// Callback for when the server sent data to a client
        /// </summary>
        /// <param name="ar"></param>
        private void ServerSendCallback(IAsyncResult ar)
        {
            // Get the socket that was sent to the callback
            Socket s = (Socket)ar.AsyncState;

            // Get the lenght of the sent bytes
            int SentLength = s.EndSend(ar);
        }

        #endregion
    }
}
