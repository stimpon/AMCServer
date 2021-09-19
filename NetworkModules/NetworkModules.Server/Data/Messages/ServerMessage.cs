/// <summary>
/// Root namespace
/// </summary>
namespace NetworkModules.Server
{
    // Required namespaces
    using System;
    using NetworkModules.Core;

    /// <summary>
    /// Information message raised by the server
    /// </summary>
    public class ServerMessage : MessageReader<ServerMessage>, IMessage
    {
        /// <summary>
        /// Gets the message source file.
        /// </summary>
        public string MessageSourceFile => Environment.CurrentDirectory + "\\Data\\Files\\ServerMessages.txt";

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }
    }
}
