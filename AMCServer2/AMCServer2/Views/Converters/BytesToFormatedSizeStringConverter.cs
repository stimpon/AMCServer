namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Globalization;
    using System.Windows;
    #endregion

    /// <summary>
    /// Converter that returns a defined static resource
    /// </summary>
    public class BytesToFormatedSizeStringConverter : BaseValueConverter<BytesToFormatedSizeStringConverter>
    {
        /// <summary>
        /// Return formated string
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
                    {
                        if (item.Size <= 1000)
                            return item.Size.ToString() + " b";
                        else if (item.Size <= 500000)
                            return ((float)item.Size / 1000).ToString("0.0") + " kb";
                        else if (item.Size <= 1000000000)
                            return ((float)item.Size / 1000000).ToString("0.00") + " mb";
                        else if (item.Size <= 1000000000000)
                            return ((float)item.Size / 1000000000).ToString("0.00") + " gb";

                        return string.Empty;
                    }
                // Folder type
                case ExplorerItemTypes.Folder:
                    return String.Empty;

                // HDD type
                case ExplorerItemTypes.HDD:
                    return String.Empty;

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
