namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.IO;
    using System.Linq;
    using NetworkModules.Core;
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
        /// All config files should exist inside Program Files/Config Files/ *Config File
        /// </summary>
        #region File paths

        /// <value>
        /// The server properties configuration file path.
        /// </value>
        public static string ServerPropertiesConfigFilePath 
            => 
            Environment.CurrentDirectory + "\\Program Files\\Config Files\\server.cfg";

        /// <summary>
        /// Gets the client privileges file path.
        /// </summary>
        public static string ClientPrivilegesFilePath
            =>
            Environment.CurrentDirectory + "\\Program Files\\Config Files\\client_privileges.cfg"; 

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
            Convert.ToInt32(File.ReadAllLines(ServerPropertiesConfigFilePath).First(l => l.StartsWith("listening_port")).Split(':')[1]);

        /// <summary>
        /// Reads the server backlog from the server config file
        /// </summary>
        /// <returns>Returns server_backlog's value</returns>
        public static int GetServerBacklog()
            =>
            Convert.ToInt32(File.ReadAllLines(ServerPropertiesConfigFilePath).First(l => l.StartsWith("server_backlog")).Split(':')[1]);

        /// <summary>
        /// Reads the buffer size from the server config file
        /// </summary>
        /// <returns>Returns buffer_size's value</returns>
        public static int GetServerBufferSize()
            =>
            Convert.ToInt32(File.ReadAllLines(ServerPropertiesConfigFilePath).First(l => l.StartsWith("buffer_size")).Split(':')[1]);

        /// <summary>
        /// Reads the FTP port from the server config file
        /// </summary>
        /// <returns>Returns buffer_size's value</returns>
        public static int GetFTPPort()
            =>
            Convert.ToInt32(File.ReadAllLines(ServerPropertiesConfigFilePath).First(l => l.StartsWith("ftp_port")).Split(':')[1]);

        #endregion
    }
}
