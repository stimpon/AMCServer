namespace AMCClient2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Globalization;
    #endregion

    public class TypeToPageConverter : BaseValueConverter<TypeToPageConverter>
    {
        /// <summary>
        /// Convert the provided type into the corresponding page
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check the provided value
            switch ((MainViews)value)
            {
                // Show a new instance of the serve interface
                case MainViews.ClientInterface:
                    return new ClientView();

                // Converter should never receive an invalid type
                default:
                    throw new Exception("Page does not exist");
            }
        }

        /// <summary>
        /// Convert back into the corresponding type
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
