namespace AMCServer2
{
    /// <summary>
    /// File handler interface
    /// </summary>
    internal interface IFileHandler
    {
        /// <summary>
        /// Gets the actual size.
        /// </summary>
        long ActualSize { get; }

        /// <summary>
        /// Gets the name of the file.
        /// </summary>
        string FileName { get; }

        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        long FileSize { get; }

        /// <summary>
        /// Gets or sets the sender.
        /// </summary>
        ClientViewModel Sender { get; set; }

        /// <summary>
        /// Writes the bytes to the file.
        /// </summary>
        /// <param name="Block">The block.</param>
        /// <returns></returns>
        bool WriteBytes(byte[] Block);
    }
}