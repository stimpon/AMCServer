/// <summary>
/// Root namespace
/// </summary>
namespace AMCServer2
{
    // Required namespaces
    using AMCCore;

    public class DownloadItemViewModel : BaseViewModel, IDownloadItem
    {
        #region Public properties

        /// <summary>
        /// Gets or sets the download identifier.
        /// </summary>
        /// <value>
        /// The download identifier.
        /// </value>
        public string DownloadID { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        /// <value>
        /// The size of the file.
        /// </value>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the downloaded bytes.
        /// </summary>
        /// <value>
        /// The downloaded bytes.
        /// </value>
        public long DownloadedBytes { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        public int ClientID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this file is downloading.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is downloading; otherwise, <c>false</c>.
        /// </value>
        public bool IsDownloading { get; set; }

        /// <summary>
        /// Progress of the download
        /// </summary>
        public string Progress { get; set; }

        #endregion

        #region Public functions

        /// <summary>
        /// Calculates the progress of the download.
        /// </summary>
        public void CalculateProgress()
        {
            Progress = (DownloadedBytes < FileSize) ?
                new string((((double)DownloadedBytes / (double)FileSize) * 100.00D).ToString("0.00") + "%") : new string("100%");

        }

        #endregion
    }
}
