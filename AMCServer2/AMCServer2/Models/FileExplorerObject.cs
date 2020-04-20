﻿namespace AMCServer2
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
        /// Tells the View if it should show the download button
        /// </summary>
        public bool CanBeDownLoaded { get => (Type == ExplorerItemTypes.File) ? true : false; }

        /// <summary>
        /// Properties that will only be set if it is a file object
        /// </summary>
        #region File properties

        /// <summary>
        /// Size of the item in bytes
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// Formats the size and returns a representable string
        /// this property is just for display purposes so that a 
        /// converter is not needed
        /// </summary>
        public string FormatedSizeString { 
            get 
            {               
                if (Type != ExplorerItemTypes.File)
                    return string.Empty;

                // Formats the string
                if (Size <= 1000)
                    return Size.ToString() + " b";
                else if (Size <= 500000)
                    return ((float)Size / 1000).ToString("0.0") + " kb";
                else if (Size <= 1000000000)
                    return ((float)Size / 1000000).ToString("0.00") + " mb";
                else if (Size <= 1000000000000)
                    return ((float)Size / 1000000000).ToString("0.00") + " gb";

                return null;
            } 
        }

        /// <summary>
        /// Extension
        /// </summary>
        public string Extension { get; set; }

        #endregion

        #region Directory properties

        /// <summary>
        /// Is true if read and write permissions are allowed
        /// </summary>
        public bool PermissionsDenied { get; set; }

        #endregion

    }
}
