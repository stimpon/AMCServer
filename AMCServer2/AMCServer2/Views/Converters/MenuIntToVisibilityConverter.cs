/// <summary>
/// Root namespace
/// </summary>
namespace AMCServer2
{
    using AMCCore;
    // Required namespaces
    using System;
    using System.Globalization;
    using System.Windows;

    public class MenuIntToVisibilityConverter : BaseValueConverter<MenuIntToVisibilityConverter>
    {
        /// <summary>
        /// This functin matches the provided converter parameter with the current menu and returns a visibility
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If the parameter can be parsed as an int and if the value is a menu
            if(int.TryParse(parameter.ToString(), out int menuInt) && Enum.TryParse<Menus>(value.ToString(), out Menus menu))
            {
                // IF this is the menu that should be visible...
                if ((Menus)menuInt == menu) return Visibility.Visible;
                // Else... This menu should be collapsed
                else return Visibility.Collapsed;
            }

            // If provided values are invalid, just return a collapsed visibility
            return Visibility.Collapsed;
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
