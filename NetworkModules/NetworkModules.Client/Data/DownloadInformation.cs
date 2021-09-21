namespace NetworkModules.Client
{
    // Required namespaces
    using NetworkModules.Core;

    /// <summary>
    /// Information abour a download
    /// </summary>
    public class DownloadInformation
    {
        /// <summary>
        /// Gets or sets a value indicating whether this download is new.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this download is new; otherwise, <c>false</c>.
        /// </value>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the ticket.
        /// </summary>
        public string Ticket { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the downloaded.
        /// </summary>
        /// <value>
        /// The downloaded.
        /// </value>
        public long Downloaded { get; set; }
    }
}
