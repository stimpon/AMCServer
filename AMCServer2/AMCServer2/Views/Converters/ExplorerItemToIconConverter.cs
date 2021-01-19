namespace AMCServer2
{
    // Required namespaces
    using System;
    using System.Globalization;
    using System.Windows;
    using AMCCore;

    /// <summary>
    /// Converter that returns a defined static resource
    /// </summary>
    public class ExplorerItemToIconConverter : BaseValueConverter<ExplorerItemToIconConverter>
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
            // Get the item
            var item = value as FileExplorerObject;

            // Check what item it is
            switch (item.Type)
            {
                case ExplorerItemTypes.File:
                    // Check what filetype it is
                    switch (item.Extension)
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

                        // Invalid file type
                        default: return Application.Current.FindResource("File"); ;
                    }

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
