namespace AMCClient2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    #endregion

    /// <summary>
    /// Class that can read and write data
    /// to all of the program's configuration
    /// files
    /// </summary>
    public static class ConfigFilesProcessor
    {
        /// <summary>
        /// Contains all file paths
        /// </summary>
        #region File paths

        /// <summary>
        /// Path to the server config file
        /// </summary>
        public static string ServerPropertiesConfigFilePath 
            => 
        Environment.CurrentDirectory + "\\Program Files\\Config Files\\client.cfg";

        #endregion

        /// <summary>
        /// Contains all methods for reading and writing data
        /// to the server config file
        /// </summary>
        #region Server config file methods

        /// <summary>
        /// Reads the listening port from the server config file
        /// </summary>
        /// <returns>Returns listening_port's value</returns>
        public static int GetServerPort()
            =>
        Convert.ToInt32(File.ReadAllLines(ServerPropertiesConfigFilePath).First(l => l.StartsWith("server_port")).Split(':')[1]);

        /// <summary>
        /// Reads the server IP address from the config file
        /// </summary>
        /// <returns>Returns buffer_size's value</returns>
        public static IPAddress GetServerIPAddress()
            =>
        IPAddress.Parse(File.ReadAllLines(ServerPropertiesConfigFilePath).First(l => l.StartsWith("server_ip")).Split(':')[1]);

        /// <summary>
        /// Reads the FTP port from the server config file
        /// </summary>
        /// <returns>Returns buffer_size's value</returns>
        public static int GetServerFTPPort()
            =>
        Convert.ToInt32(File.ReadAllLines(ServerPropertiesConfigFilePath).First(l => l.StartsWith("server_ftp_port")).Split(':')[1]);

        #endregion
    }
}
