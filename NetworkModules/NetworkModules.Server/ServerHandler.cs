namespace NetworkModules.Server
{
    // Required namespaces
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Security.Cryptography;
    using System.Linq;
    using System.IO;
    using NetworkModules.Core;
    using System.ComponentModel;

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
        /// The file transfer port
        /// </summary>
        public int FTPPort { get; set; }

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
        /// Current state of the file transfer socket
        /// </summary>
        public ServerStates FileTransferState { get; set; }

        /// <summary>
        /// Servers packet buffersize
        /// </summary>
        public int BufferSize           { get; private set; }

        /// <summary>
        /// Gets the server's bound client
        /// 
        /// This can be used if the application involves the server preforming specific
        /// operations on client PC's, for example file navigation or file transfers
        /// from clients to the server.
        /// This functionality can be ignored if no such operations will be preformed.
        /// 
        /// </summary>
        public int BoundClient          { get; private set; }

        #endregion

        #region Private members

        /// <summary>
        /// The ID that should be assigned to the next client
        /// </summary>
        private int IDs;

        /// <summary>
        /// The server socket
        /// </summary>
        private Socket ServerSocket;

        /// <summary>
        /// Path where to save downloaded files
        /// </summary>
        private string DownloadingDirectory;

        #region Downloading socket members
        /// <summary>
        /// Socket used for transfering files
        /// </summary>
        private Socket FileTransferSocket;

        /// <summary>
        /// Endpoint for the downloading socket
        /// </summary>
        private EndPoint DownloadingEndPoint;

        /// <summary>
        /// Message handler
        /// </summary>
        private Messages Messages;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// When the server has infromation for the user
        /// </summary>
        public EventHandler<InformationEventArgs> 
            NewServerInformation;

        /// <summary>
        /// When data was received from a client
        /// </summary>
        public EventHandler<ClientInformationEventArgs> 
            NewDataReceived;

        /// <summary>
        /// Carries information about a specific download
        /// </summary>
        public EventHandler<FileDownloadInformationEventArgs> 
            DownloadInformation;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="port">The server listening port</param>
        /// <param name="backlog">The server backlog</param>
        /// <param name="buffer_size">The server buffer size</param>
        public ServerHandler(int port, int ftpPort, int backlog, int buffer_size) 
        {
            // Set the listening port
            ListeningPort = port;
            // Set the file transfer port
            FTPPort = ftpPort;
            // Set the server backlog
            ServerBacklog = backlog;
            // Set the server buffer size
            BufferSize    = buffer_size;

            // Run the initialize function
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
                OnServerInformation(Messages.GetWarning(200), Responses.Warning);

                // Cancel the server setup
                return;
            } 

            // Set server states
            ServerState       = ServerStates.StartingUp;

            #region Server socket setup


            try
            {
                // Set the server endpoint
                EndPoint = new IPEndPoint(IPAddress.Any, ListeningPort);

                // Create the socket object
                ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Bind the socket to the specified port
                ServerSocket.Bind(EndPoint);
                // Begin listening
                ServerSocket.Listen(ServerBacklog);
                // Begin accepting connections async
                ServerSocket.BeginAccept(new AsyncCallback(ServerAcceptCallback), null);

                // Call the information event
                OnServerInformation(Messages.GetMessage(101), Responses.OK);
            }
            catch (SocketException se)
            {
                // Send error event
                OnServerInformation(Messages.GetErrorMessage($"{se.ErrorCode}A"), Responses.Error);
                // Set server state
                ServerState = ServerStates.Offline;

                // Return since the server could not be started
                return;
            }

            #endregion

            // Set bound client to none
            BoundClient = -1;
            // Set the serverstate to 'Running'
            ServerState = ServerStates.Online;
        }

        /// <summary>
        /// Stop the server
        /// </summary>
        public void StopServer()
        {
            // Check if server is running
            if(ServerState == ServerStates.Offline)
            {
                OnServerInformation(Messages.GetWarning("201"), Responses.Warning);
                // Do nothing
                return;
            }

            // Loop throug all connections to the server
            for (int i = 0; i < ActiveConnections.Count(); i++)
            {
                // Close the connection
                ActiveConnections[i].ClientConnection.Close();
            }

            // Close all sockets
            ServerSocket.Close();

            // Set the serverstate to offline
            ServerState = ServerStates.Offline;

            // Set the file transfer socket state to offline
            FileTransferState = ServerStates.Offline;

            // Call the information event
            OnServerInformation(Messages.GetMessage(102), Responses.OK);
        }

        /// <summary>
        /// Starts the file transfer socket.
        /// </summary>
        public void StartFileTransferSocket()
        {
            // Set the filetransfer endpoint
            DownloadingEndPoint = new IPEndPoint(IPAddress.Any, FTPPort);
            // Create downloadingsocket
            FileTransferSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Bind the file transfer socket
            FileTransferSocket.Bind(DownloadingEndPoint);
            // Begin listening
            FileTransferSocket.Listen(1);

            // Indicate that file-transfer is available
            FileTransferState = ServerStates.Online;
        }

        /// <summary>
        /// Stops the file transfer socket.
        /// </summary>
        public void StopFileTransferSocket()
        {
            // Close the file transfer socket
            FileTransferSocket.Close();

            // Indicate that file-transfer is not available
            FileTransferState = ServerStates.Offline;
        }

        /// <summary>
        /// Bind the server to a client
        /// </summary>
        /// <param name="ClientID"></param>
        public void BindServer(int ClientID)
        {
            // Loop through all clients in the active connections list
            for(int i = 0; i < ActiveConnections.Count; i++)
                // If the current user has the specified id
                if (ActiveConnections[i].ID.Equals(ClientID))
                {
                    // Set the bound client to the new client
                    BoundClient = ActiveConnections[i].ID;
                    // Send information that the client with the specified id was found
                    OnServerInformation(Messages.GetMessage(105, ClientID), Responses.OK);
                    // Break out of the loop when the client was found
                    return;
                }

            // Send information that no client with the specified id was found
            OnServerInformation(Messages.GetErrorMessage(500, ClientID), Responses.Error, false);
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
            OnServerInformation(Messages.GetMessage(103, BoundClient), Responses.OK);
            // Unbind from the bound client
            BoundClient = -1;
        }

        /// <summary>
        /// Sets the client privileges.
        /// </summary>
        /// <param name="clientID">The client identifier.</param>
        /// <param name="permissions">The privileges.</param>
        /// <returns>True if client exists and privileges was set</returns>
        public bool SetClientPrivileges(int clientID, Permissions permissions)
        {
            // Loop through all active clients
            for(int i = 0; i < ActiveConnections.Count; i++)
                // If the current client has the privided id
                if (ActiveConnections[i].ID.Equals(clientID))
                {
                    // Set the privileges for the client
                    ActiveConnections[i].ServerPermissions = permissions;
                    // return a successful result if the client was found and the privileges was set
                    return true;
                }

            // Client was not found
            return false;
        }

        /// <summary>
        /// Sends a message to a client.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">
        ///     <C> [-1]-  Sends the message to the bound client </C>
        ///     <C> [0..]- Sends the message to the specified client </C>
        /// </param>
        /// <returns>True if message was sent successfuly</returns>
        public bool Send(string message, int id = -1)
        {
            // Check if server is bound to a client
            if (BoundClient == -1 && id == -1)
            {
                OnServerInformation(Messages.GetErrorMessage(501), Responses.Error, false);
                return false; ;
            }

            // Get the bound client
            var Client = ActiveConnections.First(c => c.ID.Equals(id == -1 ? BoundClient : id));

            // Create a hasher
            var Hasher = SHA256.Create();

            // Compute and sign the hashed message
            var SignedHash = Client.Decryptor.SignHash(Hasher.ComputeHash(Encoding.Default.GetBytes(message)),
                                                                          HashAlgorithmName.SHA256,
                                                                          RSASignaturePadding.Pkcs1);
            // Send the signed hash
            Client.ClientConnection.Send(SignedHash);

            // Send the data length
            Client.ClientConnection.Send(Client.Encryptor.Encrypt( Encoding.Default.GetBytes(
                                                                   (message.Length).ToString()), 
                                                                   true));

            // Split up the data into 100 byte chunks
            for(int Length = 0; Length < message.Length; Length += 100)
            {
                // Packet that will be sent
                byte[] _packet;

                // Check if this will be the last packet
                if ((Length + 100) >= message.Length)
                    _packet = Client.Encryptor.Encrypt(
                       Encoding.Default.GetBytes(message[new Range(Length, message.Length)]), true);
                else
                    _packet = Client.Encryptor.Encrypt(
                        Encoding.Default.GetBytes(message[new Range(Length, Length + 100)]), true);

                // Send the packet
                Client.ClientConnection.Send(_packet);
            }

            return true;
        }

        /// <summary>
        /// This function needs to be called before the server can receive a file from a client
        /// </summary>
        /// <param name="clientID">Client to receive the file from</param>
        /// <param name="path">Path where to save the file</param>
        public void ReceiveFile(int clientID, string path)
        {
            // If the file-transfer socket is not online...
            if (FileTransferState != ServerStates.Online)
            {
                // Raise information event
                OnServerInformation(Messages.GetMessage(107), Responses.Information);

                // Return since we can't receive any files at the moment
                return;
            }

            // Find the client with the provided ID
            var client = ActiveConnections.FirstOrDefault(c => c.ID == c.ID);

            // If the client exists in the list of active connections
            if (client != null)
            {
                // Check if choosed path exists
                if (!Directory.Exists(path)) return;

                // Set the path
                DownloadingDirectory = path;

                // Begin accepting
                FileTransferSocket.BeginAccept(new AsyncCallback(FileTransferAcceptCallback),
                    new FTar() { Client = client, Action = FileModes.Download });
            }
            // Else if an invalid client ID was specified
            else
                // Raise error message
                OnServerInformation(Messages.GetErrorMessage(502, clientID), Responses.Error);
        }

        /// <summary>
        /// This function must be called before the server can send a file to a client
        /// </summary>
        /// <param name="filePath">The file to send</param>
        /// <param name="clientID">The cliend to send the file to</param>
        public void BeginSendFile(string filePath, int clientID)
        {
            // If the file transfer socket is not running...
            if (FileTransferState != ServerStates.Online) return; // Exit

            // If the file does not exist...
            if (!File.Exists(filePath)) return; // Exit

            // Find the client with the provided ID
            var client = ActiveConnections.FirstOrDefault(c => c.ID == clientID);

            // If a client with the provided ID exist
            if(client != null)
            {
                // Let that connection connect to the file-transfer socket
                FileTransferSocket.BeginAccept(new AsyncCallback(FileTransferAcceptCallback),
                    new FTar()
                    {
                        Client = client,
                        Action = FileModes.Send,
                        OptionalParameters = filePath
                    });
            }
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Initialize the server
        /// </summary>
        private void Initialize()
        {
            // Reset the ID counter
            IDs = 0;

            // Create the message handler
            this.Messages = new Messages();

            // Create empty connections list
            ActiveConnections = new ThreadSafeObservableCollection<ConnectionViewModel>();

            // Set serverstate to offline
            ServerState = ServerStates.Offline;
            // Set state of the file transfer socket to offline
            FileTransferState = ServerStates.Offline;
        }

        /// <summary>
        /// Sends a file to a client
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="PacketSize"></param>
        private void SendFile(string FilePath, ConnectionViewModel client, AesCryptoServiceProvider FileEncryptor)
        {
            // Create encryptor
            ICryptoTransform Crypto = FileEncryptor.CreateEncryptor();

            // Open the file
            using (FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                // Continue to read bytes until the end of the file is hit
                while (fs.Position < fs.Length)
                {
                    // Create block and fill it with bytes
                    // from the file
                    byte[] Block = new byte[512];
                    int Read = fs.Read(Block, 0, Block.Length);

                    // Resize the block to the correct size
                    Array.Resize(ref Block, Read);

                    // Encrypt the block
                    var Encrypyted = Crypto.TransformFinalBlock(Block, 0, Block.Length);

                    // Send the block to the server
                    client.FTSocket.Send(Encrypyted);
                }
            }

            // Clear key and IV from memory as it is not needed anymore
            Array.Clear(FileEncryptor.Key, 0, FileEncryptor.Key.Length);
            Array.Clear(FileEncryptor.IV, 0, FileEncryptor.IV.Length);

            // Send message stating that the file was sent to the server successfuly
            OnServerInformation(Messages.GetMessage(111, new FileInfo(FilePath).Name, client.ID), Responses.OK);
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
        /// When a client connects to the server.
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
                        ID                    = IDs,
                        ClientConnection      = s,
                        Verified              = true,
                        CurrentDataSize       = 0,
                        DataBuffer            = new byte[BufferSize],
                        CurrentDataSignature  = new byte[0],
                        ServerPermissions     = ClientDefaultData.DEFAULT_ClientPermissions
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

                    // Raise information event
                    OnServerInformation(Messages.GetMessage(100, s.RemoteEndPoint.ToString()), Responses.Information);
                }
                // If client failed to be verified
                catch (InvalidHandshakeException ex)
                {
                    // Raise information event
                    OnServerInformation(Messages.GetWarning("10001A", ex.Message), Responses.Warning);
                    // Close the connection
                    s.Close();
                }
                // If another error occurred
                catch (Exception ex)
                {
                    // Raise information event
                    OnServerInformation(Messages.GetWarning("10001B", s.RemoteEndPoint, ex.Message), Responses.Warning);
                    // Close the socket if there were any problems
                    s.Close();
                }
                // Always do
                finally
                {
                    // Increment the next if for the next client
                    IDs++;

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
            try
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

                    // Append the received bytes to the byte queue
                    _client.ByteQueue.AddRange(ReceivedBytes);

                    // Loop through all of the packets in the buffer
                    for (int PacketStart = 0; PacketStart < Rec; PacketStart += 256)
                    {
                        // Declare byte packet
                        byte[] _packet = new byte[0];

                        // If the current packet is not a whole packet (all bytes have not been received yet)
                        if (PacketStart + 256 > ReceivedBytes.Length)
                            break;

                        // Read the next block of bytes in this clients queue
                        _packet = _client.ByteQueue.GetRange(0, 256).ToArray();

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
                                    OnServerInformation(Messages.GetWarning(300, _client.ClientConnectionString, _client.CurrentDataString), 
                                        Responses.Warning);
                                }

                                // Clear the signature
                                _client.CurrentDataSignature = new byte[0];
                            }
                        }

                        // Remove the range of bytes that the server has handled
                        _client.ByteQueue.RemoveRange(0, 256);
                    }
                }

                // Begin to receive more data
                _client.ClientConnection.BeginReceive(_client.DataBuffer, 0, BufferSize,
                                                           SocketFlags.None,
                                                           new AsyncCallback(ServerReceiveCallback),
                                                           _client.ClientConnection);
            }
            // If errors occurred while receiving data from a client
            catch (Exception ex)
            {
                // Send information stating the occurred error
                OnServerInformation(ex.Message, Responses.Error);
            }
        }

        #endregion

        #region FileTransferSocket

        #region File-download callbacks

        /// <summary>
        /// Callback for when a client connects to the downloading socket
        /// </summary>
        /// <param name="ar"></param>
        private void FileTransferAcceptCallback(IAsyncResult ar)
        {
            // Define a null client
            ConnectionViewModel client = null;
            // The action to preform
            FileModes mode = FileModes.None;

            // Only resolve ar and continue if the connection came from a valid client
            try
            {
                // Get the ar
                var ftar = (FTar)ar.AsyncState;

                // Extract properties from the ar
                client = ftar.Client;
                mode = ftar.Action;

                // Set the file transfer socket
                client.FTSocket = FileTransferSocket.EndAccept(ar);
                // Create the buffer for the client
                client.DownloadBuffer = new byte[BufferSize];
                // Create the download byte queue for the client
                client.DownloadByteQueue = new System.Collections.Generic.List<byte>();
            }
            catch
            {
                // This will only happen if the connection was established from an unknow source

                // Send information stating that an unknown connection tried to connect
                OnServerInformation(Messages.GetWarning(301), Responses.Warning);

                // Exit
                return;
            }

            // If we are gonna receive a file from this client
            if(mode == FileModes.Download)
            {
                #region Declare variables

                // Create the file decryptor
                AesCryptoServiceProvider FileDecryptor = null;
                // Declare filename
                string FileName = String.Empty;
                // Declare filesize
                long FileSize = -1; 

                #endregion

                try
                {
                    // *******************************************************
                    // * If the server fails to decrypt the received AES     *
                    // * Key and IV, then we know that an invalid connection *
                    // * was established so then close the socket            *
                    // *******************************************************

                    // Variable to store received data
                    byte[] Data = new byte[256];

                    // Create decryptor
                    FileDecryptor = Cryptography.CreateAesEncryptor(false, false);

                    // Receive 128 bin AES key
                    client.FTSocket.Receive(Data);
                    FileDecryptor.Key = client.Decryptor.Decrypt(Data, true);

                    // Receive IV
                    client.FTSocket.Receive(Data);
                    FileDecryptor.IV = client.Decryptor.Decrypt(Data, true);

                    // Receive filename
                    client.FTSocket.Receive(Data);
                    FileName = Encoding.Default.GetString(client.Decryptor.Decrypt(Data, true));

                    // Receive filesize
                    client.FTSocket.Receive(Data);
                    FileSize = long.Parse(Encoding.Default.GetString(client.Decryptor.Decrypt(Data, true)));
                }

                // Close the connection if it didn't work
                catch(Exception ex)
                { 
                    // Close the connection
                    client.FTSocket.Close(); 
                    // Send error message stating what happened
                    OnServerInformation(ex.Message, Responses.Error);
                }

                // Begin receiving file from the client
                client.FTSocket.BeginReceive(client.DownloadBuffer, 0, client.DownloadBuffer.Length, SocketFlags.None,
                    new AsyncCallback(DownloadSocketReceiveCallback),
                    // Create a new FileHandler and pass it to the callback function
                    new FileDecryptorHandler(FileDecryptor,
                                             FileName,
                                             DownloadingDirectory,
                                             FileSize)
                    { Sender = client });
            }
            // If the server is gonna send a file to this client
            else if (mode == FileModes.Send)
            {
                // Create encryptor
                var FileEncryptor = Cryptography.CreateAesEncryptor();

                // Extract file from ar
                string file = ((FTar)ar.AsyncState).OptionalParameters as string;

                // Todo: Check if the file is accessable
                try
                {
                    // Try opening the file and then close it
                    using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.None)) { fs.Close(); }
                }
                // If not...
                catch
                {
                    // Close the socket
                    client.FTSocket.Close();

                    // Send information that a file that is being used by another provess if trying to be downloaded by a client
                    OnServerInformation(Messages.GetMessage(110, client.ID, file), Responses.Information);

                    // Exit
                    return;
                }

                // Send randomly generated 128 bit AES key
                client.FTSocket.Send(client.Encryptor.Encrypt(FileEncryptor.Key, true));

                // Send randomly generated IV
                client.FTSocket.Send(client.Encryptor.Encrypt(FileEncryptor.IV, true));

                // Send filename
                client.FTSocket.Send(client.Encryptor.Encrypt(Encoding.Default.GetBytes(new FileInfo(file).Name), true));

                // Send filesize
                client.FTSocket.Send(client.Encryptor.Encrypt(Encoding.Default.GetBytes((new FileInfo(file).Length.ToString())), true));

                // Send the file
                SendFile(((FTar)ar.AsyncState).OptionalParameters as string, client, FileEncryptor);

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
            int Rec = FileHandler.Sender.FTSocket.EndReceive(ar);

            // Check if any bytes where sent
            if (Rec > 0)
            {
                // Create new array from the received bytes
                byte[] ReceivedBytes = FileHandler.Sender.DownloadBuffer;
                // Resize to the correct length
                Array.Resize(ref ReceivedBytes, Rec);

                // Add the bytes recieved from the client to the byte queue for processing
                FileHandler.Sender.DownloadByteQueue.AddRange(ReceivedBytes);

                // Check that we have gotten a complete byte block
                while(FileHandler.Sender.DownloadByteQueue.Count() >= 528 ||
                   FileHandler.FileSize - FileHandler.ActualSize < 528)
                {
                    // Create an array that will hold a complete byte packet
                    byte[] currentEncryptedBlock = new byte[0];

                    // If the current packet is not the last byte packet
                    if (FileHandler.Sender.DownloadByteQueue.Count() >= 528)
                        // Get the first 32 bytes from the queue
                        currentEncryptedBlock = FileHandler.Sender.DownloadByteQueue
                            .GetRange(0, 528).ToArray();
                    // Else if this is the last byte packet
                    else
                        // Get the rest of the bytes in the queue
                        currentEncryptedBlock = FileHandler.Sender.DownloadByteQueue.ToArray();

                    // Move the byte array into a memory stream
                    using (MemoryStream mem = new MemoryStream(currentEncryptedBlock))
                        // Keep reading blocks until the end of the stream is hit
                        while (mem.Position < mem.Length)
                        {
                            // Read a block of data
                            byte[] EncryptedBlock = new byte[528];
                            int read = mem.Read(EncryptedBlock, 0, EncryptedBlock.Length);

                            // Resize the block to the correct size
                            Array.Resize(ref EncryptedBlock, read);

                            // This will be set to true if the file has been downloded after the next byte packet
                            bool fileDownloaded = false;

                            // Try to decrypt the current block
                            try
                            {
                                // Move the block into the decryption handler
                                fileDownloaded = FileHandler.WriteBytes(EncryptedBlock);

                                // Remove the handled bytes
                                FileHandler.Sender.DownloadByteQueue.RemoveRange(0, currentEncryptedBlock.Length);
                            }
                            // If decryption failed
                            catch
                            {
                                // Close the file transfer socket for this client
                                FileHandler.Sender.FTSocket.Close();

                                // Log message stating that the file could not be recieved
                                OnServerInformation(Messages.GetErrorMessage(109), Responses.Error);

                                // Todo: Cleanup

                                // Return
                                return;
                            }


                            // Call the event
                            OnDownloadInformation(FileHandler.FileName, FileHandler.FileSize, FileHandler.ActualSize);

                            // If the whole file has been received
                            if (fileDownloaded)
                            {
                                OnServerInformation(Messages.GetMessage(104, FileHandler.FileName), Responses.OK);
                                FileHandler.Sender.FTSocket.Close();
                                return;
                            }

                        }
                }
                // Keep receiving bytes
                FileHandler.Sender.FTSocket.BeginReceive(FileHandler.Sender.DownloadBuffer, 0, 
                                                              FileHandler.Sender.DownloadBuffer.Length, 
                                                              SocketFlags.None,
                                                              new AsyncCallback(DownloadSocketReceiveCallback), FileHandler);

            }
        }

        #endregion

        #region Send-file callbacks

        #endregion

        #endregion
    }
}
