﻿namespace NetworkModules.Client
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Security.Cryptography;
    using System.Text;
    using NetworkModules.Core;
    #endregion

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
        public int ServerPort           { get; private set; }

        /// <summary>
        /// The servers IP Address
        /// </summary>
        public IPAddress ServerIP       { get; private set; }

        /// <summary>
        /// Endpoint for file transfer
        /// </summary>
        public EndPoint FileTransferEndpoint { get; private set; }

        /// <summary>
        /// Servers packet buffersize
        /// </summary>
        public int GlobalBufferSize     { get; private set; }

        /// <summary>
        /// Current server staus
        /// </summary>
        public ClientStates ClientState { get; private set; }

        #endregion

        #region Private Members

        /// <summary>
        /// The server socket
        /// </summary>
        private Socket ServerConnection;

        /// <summary>
        /// Socket used for file transfer
        /// </summary>
        private Socket FileSenderSocket;

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
        /// Size of the currently receiving data
        /// </summary>
        private long CurrentDataSize;

        /// <summary>
        /// Current data stream
        /// </summary>
        private string CurrentDataString;

        #endregion

        #region Events

        /// <summary>
        /// New information event
        /// </summary>
        public EventHandler<InformationEventArgs> ClientInformation;

        /// <summary>
        /// Fires when data was received from a client
        /// </summary>
        public EventHandler<string> DataReceived;

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public ClientHandler(int port, IPAddress server_ip)
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

        #region Public functions

        /// <summary>
        /// Connect to the server
        /// </summary>
        public void Connect()
        {
            if(ClientState == ClientStates.Connected)
            {
                OnClientInformation("You are already connected to the server", Responses.Warning);
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
            // Send the data length
            ServerConnection.Send(Encryptor.Encrypt( Encoding.Default.GetBytes(
                                                     (Message.Length).ToString()), 
                                                     true));

            // Split up the data into 100 byte chunks
            for(int Length = 0; Length < Message.Length; Length += 100)
            {
                // Packet that will be sent
                byte[] _packet;

                // Check if this will be the last packet
                if ((Length + 100) >= Message.Length)
                     _packet = Encryptor.Encrypt(
                        Encoding.Default.GetBytes(Message[new Range(Length, Message.Length)]), true);
                else
                    _packet = Encryptor.Encrypt(
                        Encoding.Default.GetBytes(Message[new Range(Length, Length + 100)]), true);

                // Send the data
                ServerConnection.Send(_packet);
            }
        }

        /// <summary>
        /// Sends a file to the server
        /// </summary>
        /// <param name="FilePath"></param>
        public void BeginSendFile(string FilePath)
        {
            // Check if the client is conneted to the server
            if (ClientState != ClientStates.Connected) return;

            // Check if the file exists
            if (!File.Exists(FilePath)) return;

            // Create the socket
            FileSenderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Try to connect to the server's file transfer socket
            FileSenderSocket.BeginConnect(FileTransferEndpoint, 
                                                    new AsyncCallback(FileSenderConnectCallback),
                                                    FilePath);
        }

        #endregion

        #region Private functins

        /// <summary>
        /// Initialize the server
        /// </summary>
        private void Initialize()
        {
            // Setup server buffer
            GlobalBufferSize = 10240;
            GlobalBuffer = new byte[GlobalBufferSize];

            Decryptor = new RSACryptoServiceProvider(2048);
            Encryptor = new RSACryptoServiceProvider(2048);

            FileTransferEndpoint = new IPEndPoint(ServerIP, 401);

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
                    byte[] Block = new byte[1024];
                    int Read = fs.Read(Block, 0, Block.Length);

                    // Resize the block to the correct size
                    Array.Resize(ref Block, Read);

                    // Encrypt the block
                    var Encrypyted = Crypto.TransformFinalBlock(Block, 0, Block.Length);

                    // Send the block to the server
                    FileSenderSocket.Send(Encrypyted);
                }
            }

            // Clear key and IV from memory as it is not needed anymore
            FileEncryptor.Key.ToList().Clear();
            FileEncryptor.IV.ToList().Clear();

            // Close the FileSender socket
            FileSenderSocket.Close();
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

                OnClientInformation($"Connection to the server was established", Responses.OK);

                // Change the state
                ClientState = ClientStates.Connected;

                // Prepare file transfer socket
                FileSenderSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Begin receiving data from the server
                ServerConnection.BeginReceive(GlobalBuffer, 0, GlobalBuffer.Length,
                                              SocketFlags.None, 
                                              new AsyncCallback(ReceiveCallback),
                                              ServerConnection);
            }
            catch (Exception ex)
            {
                // Call the event
                OnClientInformation($"Connection failed, error message: {ex.Message}", Responses.Error);

                // Change back the state
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

            // This will fail if the server shutdowns
            catch
            {                 
                // Close the socket
                s.Close();

                // Call the infromation event
                OnClientInformation("You were disconnected from the server", Responses.Error);

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

                // Loop through all of the packets in the buffer (We know that the packets will be 256 bytes long)
                for(int PacketStart = 0; PacketStart < rec; PacketStart += 256)
                {
                    // Get the current packet from the buffer
                    byte[] _packet = ReceivedBytes[new Range(PacketStart, PacketStart + 256)];

                    // Check if it is the first packet in the stream
                    if (CurrentDataSize.Equals(0))
                    {
                        /* Then this packet will cantain the size
                         * of the data that will be received
                         */

                        // Get the expected data size
                        CurrentDataSize = long.Parse(Encoding.Default.GetString(
                                                      Decryptor.Decrypt(
                                                      _packet, true)));

                        // Empty the datastring
                        CurrentDataString = String.Empty;

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
                            // Call the event
                            OnDataReceived(CurrentDataString);

                            // Reset the datsize
                            CurrentDataSize = 0;
                        }
                    }
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
        private void FileSenderConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Endconnect
                FileSenderSocket.EndConnect(ar);

                // Create encryptor
                FileEncryptor = new AesCryptoServiceProvider();
                FileEncryptor.Mode         = CipherMode.CBC;
                FileEncryptor.KeySize      = 128;
                FileEncryptor.BlockSize    = 128;
                FileEncryptor.FeedbackSize = 128;
                FileEncryptor.Padding      = PaddingMode.PKCS7;

                // Create random byte generator
                using(var RNG = RandomNumberGenerator.Create())
                {
                    RNG.GetBytes(FileEncryptor.Key);
                    RNG.GetBytes(FileEncryptor.IV);
                }

                // Send randomly generated 128 bit AES key
                FileSenderSocket.Send(Encryptor.Encrypt(FileEncryptor.Key, true));

                // Send randomly generated IV
                FileSenderSocket.Send(Encryptor.Encrypt(FileEncryptor.IV, true));

                // Send filename
                FileSenderSocket.Send(Encryptor.Encrypt(Encoding.Default.GetBytes((new FileInfo((string)ar.AsyncState)).Name), true));

                // Send filesize
                FileSenderSocket.Send(Encryptor.Encrypt(Encoding.Default.GetBytes((new FileInfo((string)ar.AsyncState).Length.ToString())), true));

                // Send the file
                SendFile((string)ar.AsyncState);
            }

            // Server rejected the file transfer
            catch { OnClientInformation("Server rejected the file transfer", Responses.Error); }
        }

        #endregion
    }
}
