namespace AMCClient2
{
    /// <summary>
    /// An object that will be displayed in the explorer
    /// </summary>
    public class FileExplorerObject
    {
        /// <summary>
        /// The item name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Path of the item
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Item type
        /// </summary>
        public ExplorerItemTypes Type { get; set; }

        /// <summary>
        /// Properties that will only be set if it is a file object
        /// </summary>
        #region File properties

        /// <summary>
        /// Item size
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Extension
        /// </summary>
        public string Extension { get; set; }

        #endregion

    }
}
