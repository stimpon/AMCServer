/// <summary>
/// Root namespace
/// </summary>
namespace AMCClient2
{
    // Requiread namespaces
    using AMCCore;
    using System;
    using System.Globalization;
    using System.Windows.Media;

    /// <summary>
    /// Converts a location to a color, we want the explorer to shine a different color depending on if the file explorer
    /// is showing content on a remote PC or if it is local
    /// </summary>
    /// <seealso cref="AMCServer2.BaseValueConverter&lt;AMCServer2.Views.NavigationLocationToColorConverter&gt;" />
    public class NavigationLocationToColorConverter : BaseValueConverter<NavigationLocationToColorConverter>
    {
        /// <summary>
        /// Convert the provided value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Cast the provided value as a Navigation location
            switch ((NavigationLocations)value)
            {
                case NavigationLocations.None:
                    return Brushes.Gray;
                case NavigationLocations.Local:
                    return Brushes.Green;
                case NavigationLocations.Remote:
                    return Brushes.Red;

                default: return Brushes.White;
            }
        }

        /// <summary>
        /// Convert back into its root object
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Does not need implementation at the moment
            return null;
        }
    }
}
