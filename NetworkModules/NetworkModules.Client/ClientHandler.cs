namespace NetworkModules.Client
{
    // Required namespaces >>
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    using NetworkModules.Core;
    using System.Collections.Generic;

    /// <summary>
    /// Client backend class that handles all communication to the 
    /// AMCServer
    /// </summary>
    public class ClientHandler : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        #region Public Properties

        /// <summary>
        /// Listening port for the server
        /// </summary>
        public int ServerPort                { get; private set; }

        /// <summary>
        /// Gets the server FTP port.
        /// </summary>
        public int ServerFTPPort             { get; private set; }

        /// <summary>
        /// The servers IP Address
        /// </summary>
        public IPAddress ServerIP            { get; private set; }

        /// <summary>
        /// Endpoint for file transfer
        /// </summary>
        public EndPoint FileTransferEndpoint { get; private set; }

        /// <summary>
        /// Servers packet buffersize
        /// </summary>
        public int GlobalBufferSize          { get; private set; }

        /// <summary>
        /// Current server staus
        /// </summary>
        public ClientStates ClientState      { get; private set; }

        #endregion

        #region Private Members

        /// <summary>
        /// The server socket
        /// </summary>
        private Socket ServerConnection;

        /// <summary>
        /// Socket used for file transfer
        /// </summary>
        private Socket FileTransferSocket;

        /// <summary>
        /// This programs private key
        /// </summary>
        private RSACryptoServiceProvider Encryptor;

        /// <summary>
        /// Servers public key
        /// </summary>
        private RSACryptoServiceProvider Decryptor;

        /// <summary>
        /// Encrypter used to encrypt files
        /// </summary>
        private AesCryptoServiceProvider FileEncryptor;

        /// <summary>
        /// The server buffer
        /// </summary>
        private byte[] GlobalBuffer;

        /// <summary>
        /// The download buffer
        /// </summary>
        private byte[] DownloadBuffer;

        /// <summary>
        /// The server byte queue
        /// </summary>
        private List<byte> ServerByteQueue;

        /// <summary>
        /// The download byte queue
        /// </summary>
        private List<byte> DownloadByteQueue;

        /// <summary>
        /// Size of the currently receiving data
        /// </summary>
        private long CurrentDataSize;

        /// <summary>
        /// Signature of the message that is being received
        /// </summary>
        private byte[] CurrentSignature;

        /// <summary>
        /// Current data stream
        /// </summary>
        private string CurrentDataString;

        /// <summary>
        /// Path where to save downloaded files
        /// </summary>
        private string DownloadingDirectory;

        /// <summary>
        /// Message handler
        /// </summary>
        private Messages Messages;

        #endregion

        #region Events

        /// <summary>
        /// New information event
        /// </summary>
        public EventHandler<InformationEventArgs>
            ClientInformation;

        /// <summary>
        /// Fires when data was received from a client
        /// </summary>
        public EventHandler<string> 
            DataReceived;

        /// <summary>
        /// Carries information about a specific download
        /// </summary>
        public EventHandler<FileDownloadInformationEventArgs>
            DownloadInformation;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClientHandler(int port, IPAddress server_ip, int serverFTPPort)
        {
            /* Port cant be negative but the IPEndpoint Class does
             * not accespt uint's in the parameter
             */
            if (port <= 0)
                throw new InvalidValueException(nameof(port), port);

            // Set the server port
            ServerPort = port;
            // Set the server IP
            ServerIP = server_ip;
            // Set the server's FTP port
            ServerFTPPort = serverFTPPort;

            // First initialize
            Initialize();
        }

        #region Public functions

        /// <summary>
        /// Connect to the server
        /// </summary>
        public void Connect()
        {
            // Return if client is connected to the server
            if(ClientState == ClientStates.Connected)
            {
                // Send client message
                OnClientInformation(Messages.GetMessage(100), Responses.Warning);
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
            // If the client is disconnected from the server
            if (ClientState == ClientStates.Disconnected)
            {
                // Send client information stating that the data could not be sent
                OnClientInformation(Messages.GetWarning(401), Responses.Warning);
                // Exit
                return;
            }

            // Create a hasher
            var Hasher = SHA256.Create();

            // Compute and sign the hashed message
            var SignedHash = Decryptor.SignHash(Hasher.ComputeHash(Encoding.Default.GetBytes(Message)), 
                                                                   HashAlgorithmName.SHA256, 
                                                                   RSASignaturePadding.Pkcs1);
            // Send the signed hash
            ServerConnection.Send(SignedHash);

            var v = Encryptor.Encrypt(Encoding.Default.GetBytes(
                                                     (Message.Length).ToString()),
                                                     true);

            // Send the data length
            ServerConnection.Send(Encryptor.Encrypt( Encoding.Default.GetBytes(
                                                     (Message.Length).ToString()), 
                                                     true));

            // Split up the data into 100 byte chunks
            for(int Length = 0, index = 0; Length < Message.Length; Length += 256, index++)
            {
                // Packet that will be sent
                byte[] _packet;

                // Check if this will be the last packet
                if ((Length + 256) >= Message.Length)
                     _packet = Encryptor.Encrypt(
                        Encoding.Default.GetBytes(Message[new Range(Length, Message.Length)]), true);
                else
                    _packet = Encryptor.Encrypt(
                        Encoding.Default.GetBytes(Message[new Range(Length, Length + 256)]), true);

                // Send the data
                ServerConnection.Send(_packet);
            }
        }

        /// <summary>
        /// This function must be called before a file can be sent to the server
        /// </summary>
        /// <param name="FilePath">The file path.</param>
        public void BeginSendFile(string FilePath)
        {
            // If not connectec to the server...
            if (ClientState != ClientStates.Connected) return; // Exit

            // If the file does not exist...
            if (!File.Exists(FilePath)) return; // Exit

            // Create a new socket
            FileTransferSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the server's file transfer socket
            FileTransferSocket.BeginConnect(FileTransferEndpoint, 
                                                    new AsyncCallback(FileTransferSocketConnectCallback),
                                                    FilePath);
        }

        /// <summary>
        /// This function needs to be called before the client can receive a file from the server
        /// </summary>
        /// <param name="path">Path to where to the downloaded file</param>
        public void ReceiveFile(string path)
        {
            // If the file-transfer socket is not online...
            if (ClientState != ClientStates.Connected)
            {
                // Raise information event
                OnClientInformation(Messages.GetMessage(107), Responses.Information);
            }

            // Set the path
            DownloadingDirectory = path;

            // Prepare file transfer socket
            FileTransferSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Connect to the server's file transfer socket
            FileTransferSocket.BeginConnect(FileTransferEndpoint,
                                                    new AsyncCallback(FileTransferSocketConnectCallback),
                                                    FileModes.Download);
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Initialize the server
        /// </summary>
        private void Initialize()
        {
            // Create the message handler
            Messages = new Messages();

            // Setup server buffersize, buffer and queue
            GlobalBufferSize = 10240;
            GlobalBuffer = new byte[GlobalBufferSize];
            ServerByteQueue = new List<byte>();

            // Setup download buffer and queue
            DownloadBuffer = new byte[GlobalBufferSize];
            DownloadByteQueue = new List<byte>();

            // Setup the signature array
            CurrentSignature = new byte[0];

            Decryptor = new RSACryptoServiceProvider(2048);
            Encryptor = new RSACryptoServiceProvider(2048);

            FileTransferEndpoint = new IPEndPoint(ServerIP, ServerFTPPort);

            // Set serverstate to offline
            ClientState = ClientStates.Disconnected;
        }

        /// <summary>
        /// Sends a file to the server
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="PacketSize"></param>
        private void SendFile(string FilePath)
        {
            // Create encryptor
            ICryptoTransform Crypto = FileEncryptor.CreateEncryptor();

            // Open the file
            using(FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                // Continue to read bytes until the end of the file is hit
                while(fs.Position < fs.Length) 
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
                    FileTransferSocket.Send(Encrypyted);
                }
            }

            // Clear key and IV from memory as it is not needed anymore
            Array.Clear(FileEncryptor.Key, 0, FileEncryptor.Key.Length);
            Array.Clear(FileEncryptor.IV, 0, FileEncryptor.IV.Length);

            // Send message stating that the file was sent to the server successfuly
            OnClientInformation(Messages.GetMessage(200, new FileInfo(FilePath).Name), Responses.OK);

            // Close the FileSender socket
            FileTransferSocket.Close();
        }

        #endregion

        #region Event functions

        /// <summary>
        /// Fires when the client has new information
        /// </summary>
        /// <param name="Data"></param>
        protected virtual void OnClientInformation(string Information, Responses type)
        {
            // Check so the event is not null
            if (ClientInformation != null)
                ClientInformation(this, new InformationEventArgs() { Information = Information, MessageType = type });
        }

        /// <summary>
        /// Fires when data was received from the server
        /// </summary>
        /// <param name="Data"></param>
        protected virtual void OnDataReceived(string Data)
        {
            // Check if Event is null
            DataReceived?.Invoke(this, Data);
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

        #region ClientSocket Callbacks

        /// <summary>
        /// Connect callback
        /// </summary>
        /// <param name="ar"></param>
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
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

                OnClientInformation(Messages.GetMessage(101), Responses.OK);

                // Change the state
                ClientState = ClientStates.Connected;

                // Begin receiving data from the server
                ServerConnection.BeginReceive(GlobalBuffer, 0, GlobalBuffer.Length,
                                              SocketFlags.None, 
                                              new AsyncCallback(ReceiveCallback),
                                              ServerConnection);
            }
            catch (Exception ex)
            {
                // Send error stating that a connection to the server could not be established
                OnClientInformation(Messages.GetErrorMessage(300, ex.Message), Responses.Error);

                // Set the client state to disconnected
                ClientState = ClientStates.Disconnected;
            }

        }

        /// <summary>
        /// When the client receives data from the server
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            // Het the socket from the parameter
            Socket s = (Socket)ar.AsyncState;

            // Will hold the amount of bytes received
            int rec = 0;

            // Get the amount of bytes received
            try { rec = s.EndReceive(ar); }

            // This will fail if the server is shuting down
            catch
            {                 
                // Close the socket
                s.Close();

                // Send error stating that the client was disconnected from the server
                OnClientInformation(Messages.GetErrorMessage(301), Responses.Error);

                // Set the connection state to disconnected
                ClientState = ClientStates.Disconnected;

                // Reurn and kill this thread
                return;
            }

            // Check if empty packet was sent
            if (rec > 0)
            {
                // Create new buffer and resize it to the correct size
                byte[] ReceivedBytes = GlobalBuffer;
                Array.Resize(ref ReceivedBytes, rec);

                // Add the received bytes to the servers byte queue
                ServerByteQueue.AddRange(ReceivedBytes);

                // Loop through all of the packets in the buffer (We know that the packets will be 256 bytes long)
                for(int PacketStart = 0; PacketStart < rec; PacketStart += 256)
                {
                    // Get the current packet from the buffer
                    byte[] _packet = ReceivedBytes[new Range(PacketStart, PacketStart + 256)];

                    // If the current packet is not a whole packet (all bytes have not been received yet)
                    if (PacketStart + 256 > ReceivedBytes.Length)
                        break;

                    // Read the next bytes in the servers byte queue
                    _packet = ServerByteQueue.GetRange(0, 256).ToArray();

                    // Check if it is the first packet in the stream
                    if (CurrentDataSize.Equals(0))
                    {
                        // The signature needs to be received first
                        if (CurrentSignature.Length.Equals(0))
                        {
                            // Save the signature for comparison later
                            CurrentSignature = _packet;
                        }
                        else
                        {
                            // Get the expected data size
                            CurrentDataSize = long.Parse(Encoding.Default.GetString(
                                                         Decryptor.Decrypt(
                                                         _packet, true)));
                            // Empty the datastring
                            CurrentDataString = String.Empty;
                        }

                    }
                    // Keep adding the data to the data stream
                    else
                    {
                        // Decrypt the packet and add it to the datastring
                        CurrentDataString += Encoding.Default.GetString(
                                             Decryptor.Decrypt(
                                             _packet, true));

                        // Check if all btyes has been received
                        if (CurrentDataString.Length == CurrentDataSize)
                        {
                            // Create a hasher
                            var Hasher = SHA256.Create();

                            // Hash the received data
                            var Hash = Hasher.ComputeHash(Encoding.Default.GetBytes(CurrentDataString));

                            // Verify the signed hash with the computed hash
                            if (Encryptor.VerifyHash(Hash, CurrentSignature,
                                                           HashAlgorithmName.SHA256,
                                                           RSASignaturePadding.Pkcs1))
                            {
                                // Call the event
                                OnDataReceived(CurrentDataString);

                                // Reset the datsize
                                CurrentDataSize = 0;
                            }
                            else
                            {
                                // Send message stating that data was received but that it could not be verified
                                OnClientInformation(Messages.GetWarning(400, CurrentDataString), Responses.Warning);
                            }

                            // Clear the signature
                            CurrentSignature = new byte[0];
                        }
                    }

                    // Removed the handled bytes
                    ServerByteQueue.RemoveRange(0, 256);
                }

            }

            // Listen for more data from the server
            ServerConnection.BeginReceive(GlobalBuffer, 0, GlobalBuffer.Length,
                                          SocketFlags.None,
                                          new AsyncCallback(ReceiveCallback),
                                          s);
        }

        #endregion

        #region FileSender Callbacks

        /// <summary>
        /// When connected to the server and file is gonna be sent
        /// </summary>
        /// <param name="ar"></param>
        private void FileTransferSocketConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Endconnect
                FileTransferSocket.EndConnect(ar);

                // Get filemode
                FileModes mode = (FileModes)ar.AsyncState;

                // If the file should be sent to the server
                if(mode == FileModes.Send)
                {
                    // Try to sens the file
                    try
                    {
                        // Create encryptor
                        FileEncryptor = Cryptography.CreateAesEncryptor();

                        // Todo: Check if the file is accessable
                        try
                        {
                            // Try opening the file and then close it
                            using (FileStream fs = new FileStream((string)ar.AsyncState, FileMode.Open, FileAccess.Read, FileShare.None)) { fs.Close(); }
                        }
                        // If not...
                        catch
                        {
                            // Close the socket and return
                            FileTransferSocket.Close();

                            // Send information to the client
                            OnClientInformation(Messages.GetMessage(201, (string)ar.AsyncState), Responses.Information);

                            // Return
                            return;
                        }

                        // Send randomly generated 128 bit AES key
                        FileTransferSocket.Send(Encryptor.Encrypt(FileEncryptor.Key, true));

                        // Send randomly generated IV
                        FileTransferSocket.Send(Encryptor.Encrypt(FileEncryptor.IV, true));

                        // Send filename
                        FileTransferSocket.Send(Encryptor.Encrypt(Encoding.Default.GetBytes((new FileInfo((string)ar.AsyncState)).Name), true));

                        // Send filesize
                        FileTransferSocket.Send(Encryptor.Encrypt(Encoding.Default.GetBytes((new FileInfo((string)ar.AsyncState).Length.ToString())), true));

                        // Send the file
                        SendFile((string)ar.AsyncState);
                    }
                    // If the server rejected the file
                    catch (Exception ex)
                    {
                        // Send information stating that the file transfer was rejected
                        OnClientInformation(Messages.GetErrorMessage(302, ex.Message), Responses.Error);
                    }
                }
                // Else if the file should be downloaded
                else if (mode == FileModes.Download)
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
                        // Variable to store received data
                        byte[] Data = new byte[256];

                        // Create decryptor
                        FileDecryptor = Cryptography.CreateAesEncryptor(false, false);

                        // Receive 128 bin AES key
                        FileTransferSocket.Receive(Data);
                        FileDecryptor.Key = Decryptor.Decrypt(Data, true);

                        // Receive IV
                        FileTransferSocket.Receive(Data);
                        FileDecryptor.IV = Decryptor.Decrypt(Data, true);

                        // Receive filename
                        FileTransferSocket.Receive(Data);
                        FileName = Encoding.Default.GetString(Decryptor.Decrypt(Data, true));

                        // Receive filesize
                        FileTransferSocket.Receive(Data);
                        FileSize = long.Parse(Encoding.Default.GetString(Decryptor.Decrypt(Data, true)));
                    }

                    // Close the connection if it didn't work
                    catch (Exception ex)
                    {
                        // Close the connection
                        FileTransferSocket.Close();
                        // Send error message stating what happened
                        OnClientInformation(ex.Message, Responses.Error);
                    }

                    // Begin receiving file from the client
                    FileTransferSocket.BeginReceive(DownloadBuffer, 0, DownloadBuffer.Length, SocketFlags.None,
                        new AsyncCallback(DownloadSocketReceiveCallback),
                        // Create a new FileHandler and pass it to the callback function
                        new FileDecryptor(FileDecryptor,
                                                 FileName,
                                                 DownloadingDirectory,
                                                 FileSize));
                }
            }

            // Server rejected the file transfer
            catch(Exception ex) { }
        }

        /// <summary>
        /// When the client is receiving file bytes for a download
        /// </summary>
        /// <param name="ar"></param>
        private void DownloadSocketReceiveCallback(IAsyncResult ar)
        {
            // Filehandler from the callback
            FileDecryptor FileHandler = ar.AsyncState as FileDecryptor;

            // Get the amount of bytes received
            int Rec = FileTransferSocket.EndReceive(ar);

            // Check if any bytes where sent
            if (Rec > 0)
            {
                // Create new array from the received bytes
                byte[] ReceivedBytes = DownloadBuffer;
                // Resize to the correct length
                Array.Resize(ref ReceivedBytes, Rec);

                // Add the bytes recieved from the client to the byte queue for processing
                DownloadByteQueue.AddRange(ReceivedBytes);

                // Check that we have gotten a complete byte block
                while (DownloadByteQueue.Count >= 528 ||
                   FileHandler.FileSize - FileHandler.ActualSize < 528)
                {
                    // Create an array that will hold a complete byte packet
                    byte[] currentEncryptedBlock = new byte[0];

                    // If the current packet is not the last byte packet
                    if (DownloadByteQueue.Count >= 528)
                        // Get the first 32 bytes from the queue
                        currentEncryptedBlock = DownloadByteQueue
                            .GetRange(0, 528).ToArray();
                    // Else if this is the last byte packet
                    else
                        // Get the rest of the bytes in the queue
                        currentEncryptedBlock = DownloadByteQueue.ToArray();

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
                                DownloadByteQueue.RemoveRange(0, currentEncryptedBlock.Length);
                            }
                            // If decryption failed
                            catch
                            {
                                // Close the file transfer socket for this client
                                FileTransferSocket.Close();

                                // Log message stating that the file could not be downloaded
                                OnClientInformation(Messages.GetErrorMessage(109), Responses.Error);

                                // Todo: Cleanup

                                // Return
                                return;
                            }


                            // Call the event
                            OnDownloadInformation(FileHandler.FileName, FileHandler.FileSize, FileHandler.ActualSize);

                            // If the whole file has been downloaded
                            if (fileDownloaded)
                            {
                                OnClientInformation(Messages.GetMessage(104, FileHandler.FileName), Responses.OK);
                                FileTransferSocket.Close();
                                return;
                            }

                        }
                }
                // Keep receiving bytes
                FileTransferSocket.BeginReceive(DownloadBuffer, 0,
                                                DownloadBuffer.Length,
                                                SocketFlags.None,
                                                new AsyncCallback(DownloadSocketReceiveCallback), FileHandler);

            }
        }

        #endregion
    }
}
