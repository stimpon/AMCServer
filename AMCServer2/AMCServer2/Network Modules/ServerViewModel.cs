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
    using System.IO;
    #endregion

    /// <summary>
    /// Crypto server backend that handles AMCClients
    /// </summary>
    public class ServerViewModel : BaseViewModel
    {
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
        public string HandShakeString   { get; private set; }

        /// <summary>
        /// The client currently bound to the server
        /// </summary>
        public int BoundClient          { get; private set; }

        /// <summary>
        /// Is set to true if the server is waiting for a file
        /// or is receiving a file
        /// </summary>
        public bool IsDownloading       { get; private set; }

        #endregion

        #region Private members

        /// <summary>
        /// The server socket
        /// </summary>
        private Socket ServerSocket;

        /// <summary>
        /// Path where to save downloaded files
        /// </summary>
        private string DownloadingDirectory { get; set; }

        #region Downloading socket members
        /// <summary>
        /// Socket used for transfering files
        /// </summary>
        private Socket FileTransferSocket;

        /// <summary>
        /// Buffer for the DownloadingSocket
        /// </summary>
        private byte[] DownloadingBuffer;

        /// <summary>
        /// Endpoint for the downloading socket
        /// </summary>
        private EndPoint DownloadingEndPoint;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Fires when the server has new information
        /// </summary>
        public EventHandler<InformationEventArgs>             NewServerInformation;

        /// <summary>
        /// Fires when data was received from a client
        /// </summary>
        public EventHandler<ClientInformationEventArgs>       NewDataReceived;

        /// <summary>
        /// Fires when bytes to a file that is being downloaded is received
        /// </summary>
        public EventHandler<FileDownloadInformationEventArgs> DownloadInformation;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ServerViewModel (int port, int backlog, int buffer_size) {

            /* Port can't be negative, but the IPEndpoint Class does
             * not accespt a uint
             */
            if (port <= 0)
                throw new InvalidValueException(nameof(port), port);

            ListeningPort = port;
            ServerBacklog = backlog;
            BufferSize    = buffer_size;

            Initialize(); 
        }

        #region Public functions

        /// <summary>
        /// Start the server
        /// </summary>
        /// <returns>True if successful</returns>
        public void StartServer()
        {
            // Check if server is allready running
            if (ServerState == ServerStates.Online)
            {
                OnServerInformation("Server is already running", Responses.Warning);
                return;
            } 

            // Set server state
            ServerState = ServerStates.StartingUp;
            
            IsDownloading = false;

            // Create the socket object
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the specified port
            ServerSocket.Bind(EndPoint);
            // Begin listening
            ServerSocket.Listen(ServerBacklog);

            // Begin accepting connections async
            ServerSocket.BeginAccept(new AsyncCallback(ServerAcceptCallback), null);

            // Prepare the file transfer socket
            FileTransferSocket.Bind(DownloadingEndPoint);
            FileTransferSocket.Listen(3);

            // Startup was successful >>
            ServerState = ServerStates.Online;

            // Call the event
            OnServerInformation("Server is now running", Responses.OK);
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void StopServer()
        {
            // Return if the server is not running
            if(ServerState == ServerStates.Offline)
            {
                OnServerInformation("The server is not running", Responses.Warning);
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
            OnServerInformation("Server has been shutdown", Responses.OK);
        }

        /// <summary>
        /// Bind the server to a client
        /// </summary>
        /// <param name="ClientID"></param>
        public void BindServer(int ClientID)
        {
            try
            {
                // Try to find the client with the specified ID
                BoundClient = ActiveConnections.First(c => c.ID.Equals(ClientID)).ID;
                // Fire the information event
                OnServerInformation($"Server is now bound to client: {ClientID}", Responses.OK);
            }

            // If invalid id was specified
            catch(Exception ex)
            {
                // Fire the information event
                OnServerInformation(ex.Message, Responses.Error, false);
            }
        }

        /// <summary>
        /// Unbinds from the bound client
        /// </summary>
        public void UnbindServer()
        {
            // Return if the server is'nt bound to a client
            if (BoundClient < 0)
                return;

            // Call the event
            OnServerInformation($"Server unbinded from client {BoundClient}", Responses.OK);
            // Unbind from the bound client
            BoundClient = -1;
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
                OnServerInformation("Failed to send the request, the server is not bound to a client", Responses.Error, false);
                return false; ;
            }
            // Get the bound client
            var Client = ActiveConnections.First(c => c.ID.Equals(BoundClient));

            // Send the data length
            Client.ClientConnection.Send(Client.Encryptor.Encrypt( Encoding.Default.GetBytes(
                                                                   (Message.Length).ToString()), 
                                                                   true));

            // Split up the data into 100 byte chunks
            for(int Length = 0; Length < Message.Length; Length += 100)
            {
                // Packet that will be sent
                byte[] _packet;

                // Check if this will be the last packet
                if ((Length + 100) >= Message.Length)
                    _packet = Client.Encryptor.Encrypt(
                       Encoding.Default.GetBytes(Message[new Range(Length, Message.Length)]), true);
                else
                    _packet = Client.Encryptor.Encrypt(
                        Encoding.Default.GetBytes(Message[new Range(Length, Length + 100)]), true);

                // Send the packet
                Client.ClientConnection.Send(_packet);
            }

            return true;
        }

        /// <summary>
        /// Send a file to the specified client
        /// </summary>
        /// <param name="ClientID">Client to send the file to</param>
        /// <param name="FilePath">Path to the file</param>
        public void SendFile(int ClientID, string FilePath)
        {
        }

        /// <summary>
        /// This function allows makes so the specified
        /// client can send a file to the server
        /// </summary>
        /// <param name="ClientID">Client to receive the file from</param>
        /// <param name="Path">Path where to save the file</param>
        public void ReceiveFile(int ClientID, string Path)
        {
            // Check if it exist e client with the specified ID
            if (ActiveConnections.Select(c => c.ID).Contains(ClientID))
            {
                // Check if choosed path exists
                if (!Directory.Exists(Path)) return;

                // Set the path
                DownloadingDirectory = Path;

                // Begin accepting, this will only accept 1 
                FileTransferSocket.BeginAccept(new AsyncCallback(DownloadAcceptCallback),
                                               ClientID);
            }
            else
                OnServerInformation($"The specified client [{ClientID}] does not exist", Responses.Error);
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Initialize the server
        /// </summary>
        private void Initialize()
        {
            #region Prepare server
            // Create the endpoing
            EndPoint = new IPEndPoint(IPAddress.Any, ListeningPort);

            // Set bound client to none
            BoundClient = -1;

            // Set the handshake string
            HandShakeString = String.Empty;

            // Create empty connections list
            ActiveConnections = new ThreadSafeObservableCollection<ClientViewModel>();

            // Set serverstate to offline
            ServerState = ServerStates.Offline;
            #endregion

            #region Downloading socket setup
            // Create downloadingsocket
            FileTransferSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Create a 10mb buffer
            DownloadingBuffer = new byte[10400];

            // Create encpoint for the downloading socket
            DownloadingEndPoint = new IPEndPoint(IPAddress.Any, 401);

            #endregion
        }

        #endregion

        #region Event functions

        /// <summary>
        /// Event callback
        /// </summary>
        /// <param name="Data"></param>
        protected virtual void OnServerInformation(string Information, Responses type, bool AddTimeStamp = true)
        {
            // Check so the event is not null
            if (NewServerInformation != null)
                NewServerInformation(this, new InformationEventArgs() { Information = Information , 
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
            NewDataReceived(this, new ClientInformationEventArgs() { Client = Client, Data = Data, InformationTimeStamp = DateTime.Now.ToString() });
        }

        /// <summary>
        /// Event callback
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="FileSize"></param>
        /// <param name="ActualSize"></param>
        protected virtual void OnDownloadInformatin(string FileName, long FileSize, long ActualSize)
        {
            DownloadInformation?.Invoke(this, new FileDownloadInformationEventArgs() { FileName = FileName, FileSize = FileSize, ActualFileSize = ActualSize });
        }

        #endregion


        #region ServerSocket

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
                        ClientConnection  = s,
                        Verified          = true,
                        AutorisationLevel = 0,
                        CurrentDataSize   = 0,
                        DataBuffer        = new byte[BufferSize]
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
                    OnServerInformation($"New connection from: { s.RemoteEndPoint}", Responses.Information);
                }
                catch (InvalidHandshakeException ex)
                {
                    // Call the event
                    OnServerInformation($"{ex.Message} Tried to connect but failed to verify", Responses.Warning);
                    // Close the connection
                    s.Close();
                }
                catch (Exception ex)
                {
                    // Call the event
                    OnServerInformation($"{s.RemoteEndPoint} failed to connect, error message: {ex.Message}", Responses.Error);
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

            // Server is shutting down if this is null
            if (ClientVM == null) return;

            // Holds the amount of bytes received
            int Rec = 0;

            // Get the amount of bytes received
            try { Rec = Client.EndReceive(ar); }

            // This will fail if the client disconnects
            catch
            {
                // Call the event
                OnServerInformation($"{Client.RemoteEndPoint} disconnected", Responses.Information);
                // Close the connection
                Client.Close();
                // Remove client from the list of active connections
                ActiveConnections.Remove(ClientVM);

                // Return and kill this thread
                return;
            }

            // Check if empty byte packet was sent
            if (Rec > 0)
            {
                // Create new buffer and resize it to the correct size
                byte[] ReceivedBytes = ClientVM.DataBuffer;
                Array.Resize(ref ReceivedBytes, Rec);

                // Loop through all of the packets in the buffer (We know that the packets will be 256 bytes long)
                for (int PacketStart = 0; PacketStart < Rec; PacketStart += 256)
                {
                    // Get the current packet from the buffer
                    byte[] _packet = ReceivedBytes[new Range(PacketStart, PacketStart + 256)];

                    // Check if is is the first packet in the buffer
                    if (ClientVM.CurrentDataSize.Equals(0))
                    {
                        /* Then this packet will cantain the size
                         * of the data that will be received
                         */

                        // Get the expected data size
                        ClientVM.CurrentDataSize = long.Parse( Encoding.Default.GetString(
                                                               ClientVM.Decryptor.Decrypt(
                                                               _packet, true)));
                        // Empty the client's datastring
                        ClientVM.CurrentDataString = String.Empty;
                    }
                    // Keep adding the data to the datastring
                    else
                    {
                        // Decrypt the packet and add it to the datastring
                        ClientVM.CurrentDataString += Encoding.Default.GetString(
                                                      ClientVM.Decryptor.Decrypt(
                                                      _packet, true));

                        // Check if all btyes has been received
                        if (ClientVM.CurrentDataString.Length == ClientVM.CurrentDataSize)
                        {
                            // Call the event
                            OnDataReceived(ClientVM, ClientVM.CurrentDataString);

                            // Reset the datasize
                            ClientVM.CurrentDataSize = 0;
                        }
                    }

                }
            }

            // Begin to receive more data
            Client.BeginReceive(ClientVM.DataBuffer, 0, BufferSize,
                                                        SocketFlags.None,
                                                        new AsyncCallback(ServerReceiveCallback),
                                                        Client);
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

        #region FileTransferSocket

        /// <summary>
        /// File-download callbacks
        /// </summary>
        #region Callbacks

        /// <summary>
        /// Callback for when a client connects to the downloading socket
        /// </summary>
        /// <param name="ar"></param>
        private void DownloadAcceptCallback(IAsyncResult ar)
        {
            // Create new temp client vm
            ClientViewModel vm = new ClientViewModel() { 
                                 ClientConnection = FileTransferSocket.EndAccept(ar), 
                                 ID = (int)ar.AsyncState };

            /* If the server fails to decrypt the received AES
             * Key and IV, then we know that an invalid connection
             * was established so then close the socket
             */

            try
            {
                // Variable to store received data
                byte[] Data = new byte[256];

                // Create decryptor
                AesCryptoServiceProvider FileDecryptor = new AesCryptoServiceProvider();
                FileDecryptor.Mode         = CipherMode.CBC;
                FileDecryptor.KeySize      = 128;
                FileDecryptor.BlockSize    = 128;
                FileDecryptor.FeedbackSize = 128;
                FileDecryptor.Padding      = PaddingMode.PKCS7;

                // Receive 128 bin AES key
                vm.ClientConnection.Receive(Data);
                FileDecryptor.Key = ActiveConnections.First(c => c.ID == vm.ID).Decryptor.Decrypt(Data, true);

                // Receive IV
                vm.ClientConnection.Receive(Data);
                FileDecryptor.IV = ActiveConnections.First(c => c.ID == vm.ID).Decryptor.Decrypt(Data, true);

                // Receive filename
                vm.ClientConnection.Receive(Data);
                string FileName = Encoding.Default.GetString(ActiveConnections.First(c => c.ID == vm.ID).Decryptor.Decrypt(Data, true));

                // Receive filesize
                vm.ClientConnection.Receive(Data);
                long FileSize = long.Parse(Encoding.Default.GetString(ActiveConnections.First(c => c.ID == vm.ID).Decryptor.Decrypt(Data, true)));

                // Begin receiving file 
                vm.ClientConnection.BeginReceive(DownloadingBuffer, 0, DownloadingBuffer.Length, SocketFlags.None,
                    new AsyncCallback(DownloadSocketReceiveCallback), 
                    // Create a new FileHandler and pass it to the callback
                    new FileDecryptorHandler(FileDecryptor,
                                             FileName,
                                             DownloadingDirectory,
                                             FileSize) { Sender = vm } );
            }

            // Close the connection if it didn't work
            catch(Exception ex)
            { 
                vm.ClientConnection.Close(); 
                OnServerInformation("Invalid connection to the filetransfer socket was established, server closed the connection", Responses.Warning);
            }
        }

        /// <summary>
        /// Callback for when the server recieves data through the
        /// downloading socket
        /// </summary>
        /// <param name="ar"></param>
        private void DownloadSocketReceiveCallback(IAsyncResult ar)
        {
            // Filehandler from the callback
            FileDecryptorHandler FileHandler = ar.AsyncState as FileDecryptorHandler;

            // Get the amount of bytes received
            int Rec = FileHandler.Sender.ClientConnection.EndReceive(ar);

            // Check if any bytes where sent
            if (Rec > 0)
            {
                // Create new array from the received bytes
                byte[] ReceivedBytes = DownloadingBuffer;
                // Resize to the correct length
                Array.Resize(ref ReceivedBytes, Rec);

                // Move the byte array into a memory stream
                using(MemoryStream mem = new MemoryStream(ReceivedBytes))
                    // Keep reading blocks until the end of the stream is hit
                    while(mem.Position < mem.Length)
                    {
                        // Read a block of data
                        byte[] EncryptedBlock = new byte[1040];
                        int read = mem.Read(EncryptedBlock, 0, EncryptedBlock.Length);

                        // Resize the block to the correct size
                        Array.Resize(ref EncryptedBlock, read);

                        // Move the block into the decryption handler
                        bool res = FileHandler.WriteBytes(EncryptedBlock);

                        // Call the event
                        OnDownloadInformatin(FileHandler.FileName, FileHandler.FileSize, FileHandler.ActualSize);

                        // Check if all bytes are received
                        if (res)
                        {
                            OnServerInformation($"{FileHandler.FileName} has been downloaded", Responses.OK);
                            FileHandler.Sender.ClientConnection.Close();
                            return;
                        }

                    }
                // Keep receiving bytes
                FileHandler.Sender.ClientConnection.BeginReceive(DownloadingBuffer, 0, DownloadingBuffer.Length, SocketFlags.None,
                                          new AsyncCallback(DownloadSocketReceiveCallback), FileHandler);

            }
        }

        #endregion

        /// <summary>
        /// Send-file callbacks
        /// </summary>
        #region Callbacks

        #endregion

        #endregion
    }
}
