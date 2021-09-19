using System;
using System.Globalization;
/// <summary>
/// Root namespace
/// </summary>
namespace AMCServer2
{
    /// <summary>
    /// Converts a client ID to a representable string
    /// </summary>
    /// <seealso cref="AMCServer2.BaseValueConverter&lt;AMCServer2.ClientIDToStringConverter&gt;" />
    public class ClientIDToStringConverter : BaseValueConverter<ClientIDToStringConverter>
    {
        /// <summary>
        /// Convert the provided value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Try to parse the value to an int
            if(int.TryParse(value.ToString(), out int clientID))
            {
                // If the provided value is less then 0, then it means "None"
                if (clientID < 0) return new string("None");
                // Else just return the client id
                else return clientID;
            }
            // Else if the value could not be parsed as an int...
            else
            {
                // Return 
                return new string($"Converter received invalid value (value={value})");
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
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Does not need implementation right now
            return null;
        }
    }
}
