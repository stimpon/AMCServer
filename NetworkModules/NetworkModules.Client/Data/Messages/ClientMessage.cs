/// <summary>
/// Root namespace
/// </summary>
namespace NetworkModules.Client
{
    // Required namespaces
    using System;
    using NetworkModules.Core;

    /// <summary>
    /// 
    /// </summary>
    public class ClientMessage : MessageReader<ClientMessage>, IMessage
    {
        /// <summary>
        /// Gets the message source file.
        /// </summary>
        public string MessageSourceFile => Environment.CurrentDirectory + "\\Data\\Files\\ClientMessages.txt";

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
