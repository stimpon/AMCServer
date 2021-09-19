namespace NetworkModules.Core
{
    // Required namespaces
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a socket exception message
    /// </summary>
    public class SocketExceptionMessage : MessageReader<SocketExceptionMessage>, IMessage
    {
        /// <summary>
        /// The source file for this message
        /// </summary>
        public string MessageSourceFile => Environment.CurrentDirectory + "\\Files\\SocketExceptions.txt";

        /// <summary>
        /// Gets or sets the socket exception code.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the socket exception title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the socket exception message.
        /// </summary>
        public string Message { get; set; }

    }
}
