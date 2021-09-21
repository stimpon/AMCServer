namespace NetworkModules.Server
{
    /// <summary>
    /// Information abour a download
    /// </summary>
    public class DownloadItemViewModel
    {
        /// <summary>
        /// Gets or sets the ticket for this download (ID).
        /// </summary>
        public string Ticket { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this download is new.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this download is new; otherwise, <c>false</c>.
        /// </value>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the amount of downloaded bytes.
        /// </summary>
        /// <value>
        /// The downloaded.
        /// </value>
        public long Downloaded { get; set; }
    }
}
