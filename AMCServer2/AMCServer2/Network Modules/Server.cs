namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    #endregion

    /// <summary>
    /// Server backend class, program should only
    /// create one instance of this class
    /// </summary>
    public class Server
    {
        /// <summary>
        /// All of the public properties
        /// </summary>
        #region Properties

        /// <summary>
        /// All of the active connections
        /// </summary>
        public ThreadSafeObservableCollection<ClientViewModel> ActiveConnections { get; private set; }

        /// <summary>
        /// The port that the server will listen on
        /// </summary>
        public int ServerListeningPort  { get; private set; }

        /// <summary>
        /// Current server staus
        /// </summary>
        public ServerStates ServerState { get; private set; }

        /// <summary>
        /// Servers packet buffersize
        /// </summary>
        public int ServerBufferSize     { get; private set; }

        /// <summary>
        /// Server endpoint
        /// </summary>
        public IPEndPoint EndPoint      { get; private set; }

        /// <summary>
        /// If true, then any sockets that does not provide
        /// handshake after connecting will be dropped
        /// </summary>
        public bool 
            RequireHandShakeMessage    { get; set; }
        /// <summary>
        /// The handshake message that needs to be provided
        /// from the clien when connecting
        /// </summary>
        public string HandShakeString  { get; set; }

        #endregion

        /// <summary>
        /// All of the private members
        /// </summary>
        #region Private Members

        /// <summary>
        /// The server socket
        /// </summary>
        private Socket ServerSocket;

        /// <summary>
        /// The listening backlog
        /// </summary>
        private int ServerBacklog;

        /// <summary>
        /// The server buffer
        /// </summary>
        private byte[] ServerBuffer;

        #endregion

        /// <summary>
        /// Default constructor, sets the properties of 
        /// this server to its default settings
        /// </summary>
        public Server()
        {
            #region Declare server settings and data
            ServerListeningPort     = 400;
            ServerBufferSize        = 1024;
            ServerBacklog           = 3;
            ServerBuffer            = new byte[ServerBufferSize];
            ServerState             = ServerStates.Offline;
            RequireHandShakeMessage = false;
            HandShakeString         = String.Empty;
            ActiveConnections       = new ThreadSafeObservableCollection<ClientViewModel>();

            EndPoint                = new IPEndPoint( IPAddress.Parse("127.0.0.1"), 
                                                      ServerListeningPort);
            #endregion
        }

        #region Functions

        /// <summary>
        /// Start the server
        /// </summary>
        /// <returns>True if successful</returns>
        public bool StartServer()
        {
            // Quit if socket has not been set
            if (ServerState == ServerStates.Online)
                return false;

            // Create the socket object
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the specified port
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, ServerListeningPort));

            // Begin listening for connections
            ServerSocket.Listen(ServerBacklog);

            // Begin accepting connections async
            ServerSocket.BeginAccept(new AsyncCallback(ServerAcceptCallback), null);

            // Startup was successful >>
            ServerState = ServerStates.Online;
            return true;
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Shutdown()
        {
            ServerState = ServerStates.Offline;

            // Closer all actice connections
            foreach (var Client in ActiveConnections)
                Client.ClientConnection.Close();
            ActiveConnections.Clear();

            // Close the socket
            ServerSocket.Close();
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

            // Wait for handshake if set to do so
            if (RequireHandShakeMessage) { s.Receive(new byte[HandShakeString.Length]); }

            // Create the Client obejct
            var ClientConnection = new ClientViewModel()
            {
                ClientConnection = s,
                AutorisationLevel = 0
            };

            // Begin listening to the client
            s.BeginReceive(ServerBuffer, 0, ServerBuffer.Length,
                                                            SocketFlags.None,
                                                            new AsyncCallback(ServerReceiveCallback),
                                                            ClientConnection.ClientConnection);

            // Add the new connection to the list of connections
            ActiveConnections.Add(ClientConnection);

            // Begin accepting more connections
            ServerSocket.BeginAccept(new AsyncCallback(ServerAcceptCallback), null);
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
                Socket Client = (Socket)ar.AsyncState;

                // Check the lengt of the sent data
                int Rec = Client.EndReceive(ar);

                // Check if an empty packet was received
                if (Rec > 0)
                {
                    // Create new buffer and resize it to the correct size
                    byte[] ReceivedBytes = ServerBuffer;
                    Array.Resize(ref ReceivedBytes, Rec);

                    // Convert the bytes to a string
                    string Message = Encoding.Default.GetString(ReceivedBytes);
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

        #endregion
    }
}
