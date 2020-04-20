using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AMCServer2
{
    /// <summary>
    /// This is a ViewModel of a local file, 
    /// ViewModel implements ILogMessage so that
    /// it can be displayed in the server log.
    /// </summary>
    public class FileViewModel : BaseViewModel
    {
        /// <summary>
        /// Implements ILogMessage so it can be displayed in the
        /// server log
        /// </summary>
        #region Interface

        /// <summary>
        /// When the file beggan downloading
        /// </summary>
        public string EventTime { get; set; }

        /// <summary>
        /// This will show the status of the download
        /// </summary>
        public string Content { get; set; }

        public bool ShowTime { get; set; } = false;

        public Responses Type { get; set; } = Responses.Information;

        #endregion


        #region Public properties
        /// <summary>
        /// ID
        /// </summary>
        public int ID                 { get; private set; }

        /// <summary>
        /// Name of the file being downloaded
        /// </summary>
        public string FileName        { get; set; }

        /// <summary>
        /// Local path to the file
        /// </summary>
        public string FilePath        { get; set; }

        /// <summary>
        /// Size of the file
        /// </summary>
        public double FileSize        { get; set; }

        /// <summary>
        /// How much of the file has been downloaded
        /// </summary>
        public double BytesDownloaded { get; set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public FileViewModel(string FileName, string FilePath, double FileSize)
        {
            this.FileName = FileName;
            this.FilePath = FilePath;
            this.FileSize = FileSize;

            BytesDownloaded = 334;

            UpdateMessage();
        }

        /// <summary>
        /// Updates the content of the message
        /// </summary>
        private void UpdateMessage()
        {
            // Calculate how many procent has been downloaded
            double percentage_complete = (BytesDownloaded / FileSize) * 100;

            // Update the content that will be sisplayed in the server log
            if (percentage_complete != 100)
                Content = $"Downloading: {FileName}, {percentage_complete.ToString("0.0")}%";
            else
                Content = $"";
            
        }

        /// <summary>
        /// Writes a byte array to the ens of that file
        /// </summary>
        public void WriteBytesToFile(byte[] bytes)
        {
            if (!File.Exists(FilePath)) return;

            // Create a FileStream object that will be used to write the bytes to the file
            using(FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Write))
            {
                // Write the bytes to the file
                fs.Write(bytes, 0, bytes.Length);
            }

            // Updates the message that is displayed in the server log
            UpdateMessage();
        }

        /// <summary>
        /// Hashes the file with SHA256 and returns the hash string
        /// </summary>
        /// <returns></returns>
        public string GetFileHash()
        {
            // Check if the file even exists
            if (File.Exists($"{FilePath}\\{FileName}"))
            {
                // Create hasher
                SHA256 Hasher = SHA256.Create();

                // Compute hash and return hash as a string
                return BitConverter.ToString(
                    Hasher.ComputeHash( File.ReadAllBytes(FilePath) ));
            }

            // Return null if it does not exist
            else return null;
        }
    }
}
