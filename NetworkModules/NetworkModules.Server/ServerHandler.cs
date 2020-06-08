namespace NetworkModules.Server
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Security.Cryptography;
    using System.Linq;
    using System.IO;
    using NetworkModules.Core;
    using System.ComponentModel;
    #endregion

    /// <summary>
    /// Crypto server backend that handles AMCClients
    /// </summary>
    public class ServerHandler : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

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
        public ThreadSafeObservableCollection<ConnectionViewModel> ActiveConnections { get; private set; }

        /// <summary>
        /// Current server staus
        /// </summary>
        public ServerStates ServerState { get; private set; }

        /// <summary>
        /// Servers packet buffersize
        /// </summary>
        public int BufferSize           { get; private set; }

        /// <summary>
        /// The client currently bound to the server
        /// </summary>
        public int BoundClient          { get; private set; }

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
        /// When the server has infromation for the user
        /// </summary>
        public EventHandler<InformationEventArgs> NewServerInformation;

        /// <summary>
        /// When data was received from a client
        /// </summary>
        public EventHandler<ClientInformationEventArgs> NewDataReceived;

        /// <summary>
        /// Carries information about a specific download
        /// </summary>
        public EventHandler<FileDownloadInformationEventArgs> DownloadInformation;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ServerHandler(int port, int backlog, int buffer_size) {

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
            if (ServerState != ServerStates.Offline)
            {
                // Call the information event
                OnServerInformation("Server is already running", Responses.Warning);

                // Cancel the server setup
                return;
            } 

            // Set server state
            ServerState = ServerStates.StartingUp;

            #region Server socket setup

            // Create the socket object
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bind the socket to the specified port
            ServerSocket.Bind(EndPoint);
            // Begin listening
            ServerSocket.Listen(ServerBacklog);
            // Begin accepting connections async
            ServerSocket.BeginAccept(new AsyncCallback(ServerAcceptCallback), null);

            #endregion

            #region Filetransfer socket setup

            // Create downloadingsocket
            FileTransferSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);            
            // Bind the file transfer socket
            FileTransferSocket.Bind(DownloadingEndPoint);
            // Begin listening
            FileTransferSocket.Listen(3);
            // Create a 10mb downloading-buffer
            DownloadingBuffer = new byte[10400];

            #endregion

            // Set bound client to none
            BoundClient = -1;
            // Set the serverstate to 'Running'
            ServerState = ServerStates.Online;
            // Call the information event
            OnServerInformation("Server is now running", Responses.OK);
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void StopServer()
        {
            // Check if server is running
            if(ServerState == ServerStates.Offline)
            {
                OnServerInformation("The server is not running", Responses.Warning);
                // Do nothing
                return;
            }

            // Closer all active connections
            foreach (var Client in ActiveConnections)
                Client.ClientConnection.Close();

            // Close all sockets
            ServerSocket.Close();
            FileTransferSocket.Close();

            // Set the serverstate to offline
            ServerState = ServerStates.Offline;

            // Call the information event
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

            // Create a hasher
            var Hasher = SHA256.Create();

            // Compute and sign the hashed message
            var SignedHash = Client.Decryptor.SignHash(Hasher.ComputeHash(Encoding.Default.GetBytes(Message)),
                                                                          HashAlgorithmName.SHA256,
                                                                          RSASignaturePadding.Pkcs1);
            // Send the signed hash
            Client.ClientConnection.Send(SignedHash);

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
            // Set the server endpoint
            EndPoint = new IPEndPoint(IPAddress.Any, ListeningPort);
            // Set the filetransfer endpoint
            DownloadingEndPoint = new IPEndPoint(IPAddress.Any, 401); 
            // Create empty connections list
            ActiveConnections = new ThreadSafeObservableCollection<ConnectionViewModel>();

            // Set serverstate to 'offline'
            ServerState = ServerStates.Offline;
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
        protected virtual void OnDataReceived(ConnectionViewModel Client, string Data)
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
        protected virtual void OnDownloadInformation(string FileName, long FileSize, long ActualSize)
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
                    // Create the Client obejct
                    var ClientConnection = new ConnectionViewModel()
                    {
                        ClientConnection  = s,
                        Verified          = true,
                        AutorisationLevel = 0,
                        CurrentDataSize   = 0,
                        DataBuffer        = new byte[BufferSize],
                        CurrentDataSignature = new byte[0]
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
                        ClientConnection.Encryptor.ImportRSAPublicKey(PK, out int _ );
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
            // Get the conenction object from the list of active connections
            var _client = ActiveConnections.FirstOrDefault(c => c.ClientConnection.Equals((Socket)ar.AsyncState));

            // If server is shuting down
            if (_client.ClientConnection.Connected == false) 
            {
                // Close the connection
                _client.ClientConnection.Close();
                // Remove client from the list of active connections
                ActiveConnections.Remove(_client);

                // Return and kill this thread
                return;
            }

            // Holds the amount of bytes received
            int Rec = _client.ClientConnection.EndReceive(ar);

            // Check if empty byte packet was sent
            if (Rec > 0)
            {
                // Create new buffer and resize it to the correct size
                byte[] ReceivedBytes = _client.DataBuffer;
                Array.Resize(ref ReceivedBytes, Rec);

                // Loop through all of the packets in the buffer (We know that the packets will be 256 bytes long)
                for (int PacketStart = 0; PacketStart < Rec; PacketStart += 256)
                {
                    // Get the current packet from the buffer
                    byte[] _packet = ReceivedBytes[new Range(PacketStart, PacketStart + 256)];

                    // Check if is is the first packet in the buffer
                    if (_client.CurrentDataSize.Equals(0))
                    {

                        // The signature needs to be received first
                        if (_client.CurrentDataSignature.Length.Equals(0))
                        {
                            // Save the signature for comparison later
                            _client.CurrentDataSignature = _packet;
                        }
                        else
                        {
                            // Get the expected data size
                            _client.CurrentDataSize = long.Parse(Encoding.Default.GetString(
                                                                  _client.Decryptor.Decrypt(
                                                                  _packet, true)));
                            // Empty the client's datastring
                            _client.CurrentDataString = String.Empty;
                        }
                    }
                    // Keep adding the data to the datastring
                    else
                    {
                        // Decrypt the packet and add it to the datastring
                        _client.CurrentDataString += Encoding.Default.GetString(
                                                      _client.Decryptor.Decrypt(
                                                      _packet, true));

                        // Check if all btyes has been received
                        if (_client.CurrentDataString.Length == _client.CurrentDataSize)
                        {
                            // Create a hasher
                            var Hasher = SHA256.Create();

                            // Hash the received data
                            var Hash = Hasher.ComputeHash(Encoding.Default.GetBytes(_client.CurrentDataString));

                            // Verify the signed hash with the computed hash
                            if (_client.Encryptor.VerifyHash(Hash, _client.CurrentDataSignature,
                                                             HashAlgorithmName.SHA256,
                                                             RSASignaturePadding.Pkcs1))
                            {
                                // Call the event
                                OnDataReceived(_client, _client.CurrentDataString);

                                // Reset the datasize
                                _client.CurrentDataSize = 0;
                            }
                            else
                            {
                                // Send a warning
                                OnServerInformation($"A message was received from {_client.ClientConnectionString} but the received " +
                                    $"signature could not be verified.\nData: {_client.CurrentDataString}", Responses.Warning);
                            }

                            // Clear the signature
                            _client.CurrentDataSignature = new byte[0];
                        }
                    }

                }
            }

            // Begin to receive more data
            _client.ClientConnection.BeginReceive(_client.DataBuffer, 0, BufferSize,
                                                       SocketFlags.None,
                                                       new AsyncCallback(ServerReceiveCallback),
                                                       _client.ClientConnection);
        }

        #endregion

        #region FileTransferSocket

        #region File-download callbacks

        /// <summary>
        /// Callback for when a client connects to the downloading socket
        /// </summary>
        /// <param name="ar"></param>
        private void DownloadAcceptCallback(IAsyncResult ar)
        {
            // Create new temp client vm
            ConnectionViewModel vm = new ConnectionViewModel() { 
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
                                             FileSize) { Sender = vm.ClientConnection } );;
            }

            // Close the connection if it didn't work
            catch
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
            int Rec = FileHandler.Sender.EndReceive(ar);

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
                        OnDownloadInformation(FileHandler.FileName, FileHandler.FileSize, FileHandler.ActualSize);

                        // Check if all bytes are received
                        if (res)
                        {
                            OnServerInformation($"{FileHandler.FileName} has been downloaded", Responses.OK);
                            FileHandler.Sender.Close();
                            return;
                        }

                    }
                // Keep receiving bytes
                FileHandler.Sender.BeginReceive(DownloadingBuffer, 0, DownloadingBuffer.Length, SocketFlags.None,
                                          new AsyncCallback(DownloadSocketReceiveCallback), FileHandler);

            }
        }

        #endregion

        #region Send-file callbacks

        #endregion

        #endregion
    }
}
