namespace NetworkModules.Client
{
    // Required mamespaces
    using System;

    /// <summary>
    /// EventArgs that carries information about a download
    /// </summary>
    public class FileDownloadInformationEventArgs : EventArgs
    {
        /// <summary>
        /// Information about the download
        /// </summary>
        public DownloadInformation Download { get; set; }

        /// <summary>
        /// Timestamp
        /// </summary>
        public string InformationTimeStamp { get; set; }
    }
}
