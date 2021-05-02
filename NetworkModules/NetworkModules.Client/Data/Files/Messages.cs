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
    public class Messages : MessagesHandler
    {
        /// <summary>
        /// Gets the message file path.
        /// </summary>
        public override string MessageFilePath =>
            Environment.CurrentDirectory + "\\Data\\Files\\Messages";
    }
}
