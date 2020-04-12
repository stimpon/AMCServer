namespace AMCServer2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Globalization;
    using System.Windows.Media;
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
            switch ((InformationTypes)value)
            {        
                // If it is just information, use white
                case InformationTypes.Information:      return new SolidColorBrush(Colors.White);

                // If it is a warning, use orange
                case InformationTypes.Warning:          return new SolidColorBrush(Colors.Orange);

                // If it is an error message, use red
                case InformationTypes.Error:            return new SolidColorBrush(Colors.Red);

                // If it is an action failed
                case InformationTypes.ActionFailed:     return new SolidColorBrush(Colors.Red);

                // If an action was successful, use gree,
                case InformationTypes.ActionSuccessful: return new SolidColorBrush(Colors.ForestGreen);

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
