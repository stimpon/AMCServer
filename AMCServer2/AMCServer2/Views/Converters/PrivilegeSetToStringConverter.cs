namespace AMCServer2
{
    // Requried namespaces
    using System;
    using System.Globalization;
    using NetworkModules.Core;

    /// <summary>
    /// Coverter that converts a privilege-set to a representable string
    /// </summary>
    /// <seealso cref="AMCServer2.BaseValueConverter{AMCServer2.Views.Converters.PrivilegeSetToStringConverter}" />
    public class PrivilegeSetToStringConverter : BaseValueConverter<PrivilegeSetToStringConverter>
    {
        /// <summary>
        /// Convert the provided value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Create the result string
            string privilegeString = ((Permissions)value).ToString();

            // Return the new string
            return privilegeString;
        }

        /// <summary>
        /// Convert back into its root object
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Does not need implementation for now
            throw new NotImplementedException();
        }
    }
}
