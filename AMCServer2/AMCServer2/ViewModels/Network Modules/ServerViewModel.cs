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
    using System.Collections.ObjectModel;
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
        public int ListeningPort   { get; private set; }

        /// <summary>
        /// Server backlog
        /// </summary>
        public int ServerBacklog   { get; private set; }

        /// <summary>
        /// Server IPEndPoint
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// The server socket
        /// </summary>
        public Socket ServerSocket { get; private set; }

        /// <summary>
        /// All of the active connections
        /// </summary>
        public ThreadSafeObservableCollection<ClientViewModel> ActiveConnections { get; private set; }

        /// <summary>
        /// Current server staus
        /// </summary>
        public ServerStates ServerState { get; private set; }

        /// <summary>
        /// Servers packet buffersize
        /// </summary>
        public int BufferSize           { get; private set; }

        /// <summary>
        /// The handshake message that needs to be provided
        /// from the clien when connecting
        /// </summary>
        public string HandShakeString   { get; set; }

        /// <summary>
        /// The client currently bound to the server
        /// </summary>
        public int BoundClient          { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Fires when the server has new information
        /// </summary>
        public EventHandler<InformationEventArgs>  ServerInformation;

        /// <summary>
        /// Fires when data was received from a client
        /// </summary>
        public EventHandler<ClientInformationEventArgs>  DataReceived;

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
            BufferSize = 10240;

            EndPoint = new IPEndPoint(IPAddress.Any, ListeningPort);

            BoundClient = -1;

            // Set the handshake string
            HandShakeString         = String.Empty;

            // Create empty connections list
            ActiveConnections       = new ThreadSafeObservableCollection<ClientViewModel>();
            
            // Set serverstate to offline
            ServerState = ServerStates.Offline;
        }

        /// <summary>
        /// Start the server
        /// </summary>
        /// <returns>True if successful</returns>
        public void StartServer()
        {
            if (ServerState == ServerStates.Online)
            {
                OnServerInformation("Server is already running", InformationTypes.Warning);
                return;
            } 

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

            // Call the event
            OnServerInformation("Server is now running", InformationTypes.ActionSuccessful);
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void Shutdown()
        {
            // Return if the server is not running
            if(ServerState == ServerStates.Offline)
            {
                OnServerInformation("The server is not running", InformationTypes.Warning);
                return;
            }

            // Closer all actice connections
            foreach (var Client in ActiveConnections)
                Client.ClientConnection.Close();

            ActiveConnections.Clear();

            // Close the socket
            ServerSocket.Close();

            // Startup complete >>
            ServerState = ServerStates.Offline;

            // Call the event
            OnServerInformation("Server has been shutdown", InformationTypes.ActionSuccessful);
        }

        /// <summary>
        /// Bind the server to a client
        /// </summary>
        /// <param name="ClientID"></param>
        public void Bind(int ClientID)
        {
            try
            {
                // Try to find the client with the specified ID
                BoundClient = ActiveConnections.First(c => c.ID.Equals(ClientID)).ID;
                // Fire the information event
                OnServerInformation($"Server is now bound to client: {ClientID}", InformationTypes.ActionSuccessful);
            }

            // If invalid id was specified
            catch(Exception ex)
            {
                // Fire the information event
                OnServerInformation(ex.Message, InformationTypes.ActionFailed, false);
            }
        }

        /// <summary>
        /// Send data to a bound client
        /// </summary>
        /// <param name="Message"></param>
        public bool Send(string Message)
        {
            // Check if server is bound to a client
            if (BoundClient < 0)
            {
                OnServerInformation("Server is not bound to a client", InformationTypes.ActionFailed, false);
                return false; ;
            }
            // Get the bound client
            var Client = ActiveConnections.First(c => c.ID.Equals(BoundClient));
            // Encrypt and get the bytes from the string
            byte[] Bytes = Encoding.Default.GetBytes(Message);
            byte[] EncryptedBytes;

            if (Bytes.Length >= 200)
            {

            }
            else
            {
                EncryptedBytes = Client.Encryptor.Encrypt(Bytes, true);
                // Send the message
                Client.ClientConnection.BeginSend(EncryptedBytes, 0, EncryptedBytes.Length,
                                                  SocketFlags.None,
                                                  new AsyncCallback(ServerSendCallback),
                                                  Client.ClientConnection);
            }

            return true;
        }

        #region Events

        /// <summary>
        /// Event callback
        /// </summary>
        /// <param name="Data"></param>
        protected virtual void OnServerInformation(string Information, InformationTypes type, bool AddTimeStamp = true)
        {
            // Check so the event is not null
            if (ServerInformation != null)
                ServerInformation(this, new InformationEventArgs() { Information = Information , 
                                                                     MessageType = type, 
                                                                     InformationTimeStamp = (AddTimeStamp) ? DateTime.Now.ToString(): null });
        }

        /// <summary>
        /// Event callback
        /// </summary>
        /// <param name="Client"></param>
        protected virtual void OnDataReceived(ClientViewModel Client, string Data)
        {
            // Fire the event
            DataReceived(this, new ClientInformationEventArgs() { Client = Client, Data = Data, InformationTimeStamp = DateTime.Now.ToString() });
        }

        #endregion

        #endregion


        #region Socket callbacks

        /// <summary>
        /// When a client has connected
        /// </summary>
        /// <param name="ar"></param>
        private void ServerAcceptCallback(IAsyncResult ar)
        {
            try
            {
                // This is the connection that has been made
                Socket s = ServerSocket.EndAccept(ar);

                try
                {
                    byte[] Handshake = new byte[4];
                    s.Receive(Handshake);
                    if (Encoding.Default.GetString(Handshake) != "[VF]")
                        throw new InvalidHandshakeException(s);

                    // Create the Client obejct
                    var ClientConnection = new ClientViewModel()
                    {
                        ClientConnection = s,
                        Verified = true,
                        AutorisationLevel = 0,
                        DataBuffer = new byte[BufferSize]
                    };

                    // Create placeholder for the clients public key
                    ClientConnection.Encryptor = new RSACryptoServiceProvider(2048);
                    // Create a unique RSA keypair for this client
                    ClientConnection.Decryptor = new RSACryptoServiceProvider(2048);

                    // Send server's public key >>
                    s.Send(ClientConnection.Decryptor.ExportRSAPublicKey());

                    // Set key exchange timeout
                    s.ReceiveTimeout = 5000;

                    // Receive Clients's public key
                    { // >>
                        byte[] PK = new byte[500];
                        int len = s.Receive(PK);

                        // Resize to the correct size before importing
                        Array.Resize(ref PK, len);
                        ClientConnection.Encryptor.ImportRSAPublicKey(PK, out int l);
                    } // <<

                    // Begin listening to the client
                    s.BeginReceive(ClientConnection.DataBuffer, 0, BufferSize,
                                                    SocketFlags.None,
                                                    new AsyncCallback(ServerReceiveCallback),
                                                    ClientConnection.ClientConnection);

                    // Add the new connection to the list of connections
                    ActiveConnections.Add(ClientConnection);

                    // Call the event
                    OnServerInformation($"New connection from: { s.RemoteEndPoint.ToString()}", InformationTypes.Information);
                }
                catch (InvalidHandshakeException ex)
                {
                    // Call the event
                    OnServerInformation($"{s.RemoteEndPoint.ToString()} Tried to connect but failed to verify", InformationTypes.Warning);
                    // Close the connection
                    s.Close();
                }
                catch (Exception ex)
                {
                    // Call the event
                    OnServerInformation($"{s.RemoteEndPoint} failed to connect, error message: {ex.Message}", InformationTypes.Error);
                    // Close the socket if there were any problems
                    s.Close();
                }
                finally
                {
                    // Begin accepting more connections
                    ServerSocket.BeginAccept(new AsyncCallback(ServerAcceptCallback), null);
                }
            }

            // Should only come here if the server is being shutdown
            catch { }
        }

        /// <summary>
        /// When the server receives data from a client
        /// </summary>
        /// <param name="ar"></param>
        private void ServerReceiveCallback(IAsyncResult ar)
        {
            // Get the socket
            Socket Client = (Socket)ar.AsyncState;
            // Get the conenction object from the list of active connections
            var ClientVM = ActiveConnections.FirstOrDefault(c => c.ClientConnection.Equals(Client));
            // Server is shutting down
            if (ClientVM == null) return;

            // This can fail if client disconnects
            try
            {
                // Check the lengt of the sent data
                int Rec = Client.EndReceive(ar);

                // Check if empty byte packet was sent
                if(Rec > 0)
                {
                    // Create new buffer and resize it to the correct size
                    byte[] ReceivedBytes = ClientVM.DataBuffer;
                    Array.Resize(ref ReceivedBytes, Rec);

                    // byte[512] > 511

                    // Loop through all of the packets
                    for (int PacketStart = 0; PacketStart < Rec; PacketStart += 256)
                    {
                        // Read the current bytepacket
                        byte[] _packet = ReceivedBytes[new Range(PacketStart, PacketStart + 256)];

                        // Decrypt and convert the received bytes to a string
                        try
                        {
                            // Encrypt the bytes and retrieve the string
                            string Message = Encoding.Default.GetString(
                                             ClientVM.Decryptor.Decrypt(
                                             _packet, true)
                                             );
                            // Call the event
                            OnDataReceived(ClientVM, Message);
                        }
                        catch
                        {
                            OnServerInformation($"{Client.RemoteEndPoint.ToString()} sent data that was not possible to decrypt with the server's public key.\n Data: " +
                                                $"{Encoding.Default.GetString(ReceivedBytes)}",
                                                InformationTypes.Warning);
                            break;
                        }

                    }
                }

                // Begin receiving again
                Client.BeginReceive(ClientVM.DataBuffer, 0, BufferSize,
                                                            SocketFlags.None,
                                                            new AsyncCallback(ServerReceiveCallback),
                                                            Client);
            }
            // Remove the connection
            catch 
            {
                // Call the event
                OnServerInformation($"{Client.RemoteEndPoint.ToString()} disconnected", InformationTypes.Information);               
                // Close the connection
                Client.Close();
                // Remove client from the list of active connections
                ActiveConnections.Remove(ClientVM);
            }
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
