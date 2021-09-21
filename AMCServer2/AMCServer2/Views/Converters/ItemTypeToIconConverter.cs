namespace AMCServer2
{
    // Required namespaces
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using AMCCore;

    /// <summary>
    /// Converter that returns a defined static resource
    /// </summary>
    public class FileTypeToIconConverter : BaseValueConverter<FileTypeToIconConverter>
    {
        /// <summary>
        /// Return the correct resource
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Get the item type
            var type = value.GetType();

            // If this is a file explorer item
            if(type == typeof(FileExplorerObject))
            {
                // Get the item
                var item = value as FileExplorerObject;

                // Check what item it is
                switch (item.Type)
                {
                    case ExplorerItemTypes.File:
                        // Check what filetype it is
                        return ResolveExtension(item.Extension);

                    // Folder type
                    case ExplorerItemTypes.Folder:
                        return Application.Current.FindResource("Folder");

                    // HDD type
                    case ExplorerItemTypes.HDD:
                        return Application.Current.FindResource("HDD");

                    // Invalid item type
                    default: throw new NullReferenceException();
                }

            }

            // If this is a download item
            if(type == typeof(DownloadItemViewModel))
            {
                // Get the item
                var item = value as DownloadItemViewModel;
                // Check extension and return icon
                return ResolveExtension(Path.GetExtension(item.FileName));
            }

            // Return null if invalid value was provided
            return null;
        }

        /// <summary>
        /// Resolves the extension.
        /// </summary>
        /// <returns></returns>
        private object ResolveExtension(string extension)
        {
            // Check extension
            switch (extension)
            {
                // Text files
                case ".txt": return Application.Current.FindResource("FileText");

                // Archive files
                case ".rar": return Application.Current.FindResource("FileArchive");
                case ".zip": return Application.Current.FindResource("FileArchive");

                // Audio files
                case ".mp3": return Application.Current.FindResource("FileAudio");
                case ".wav": return Application.Current.FindResource("FileAudio");

                // Video files
                case ".mp4": return Application.Current.FindResource("FileVideo");
                case ".mkv": return Application.Current.FindResource("FileVideo");
                case ".avi": return Application.Current.FindResource("FileVideo");

                case ".png": return Application.Current.FindResource("FilePhoto");
                case ".jpg": return Application.Current.FindResource("FilePhoto");

                // Invalid file type
                default: return Application.Current.FindResource("File"); ;
            }
        }

        /// <summary>
        /// Convert back to the file type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
