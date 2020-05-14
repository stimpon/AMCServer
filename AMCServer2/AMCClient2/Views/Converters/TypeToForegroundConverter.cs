namespace AMCClient2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Globalization;
    using System.Windows.Media;
    using NetworkModules.Core;
    #endregion

    /// <summary>
    /// Covnerts the type of log to the correct foreground
    /// </summary>
    public class TypeToForegroundConverter : BaseValueConverter<TypeToForegroundConverter>
    {
        /// <summary>
        /// Convert to the corresponding foreground
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check what the log item type is
            switch ((Responses)value)
            {        
                // If it is just information, use white
                case Responses.Information:      return new SolidColorBrush(Colors.White);

                // If it is a warning, use orange
                case Responses.Warning:          return new SolidColorBrush(Colors.Orange);
                // If it is an error message, use red
                case Responses.Error:            return new SolidColorBrush(Colors.Red);
                // If an action was successful, use gree,
                case Responses.OK: return new SolidColorBrush(Colors.ForestGreen);

                // If no type was provided, use gray
                default: return new SolidColorBrush(Colors.Gray);
            }
        }

        /// <summary>
        /// Convert back to the correspinding type
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Does not need implementation
            throw new NotImplementedException();
        }
    }
}
