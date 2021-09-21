namespace AMCServer2
{
    // Required namespaces
    using System;
    using AMCServer;
    using System.Globalization;
    using AMCCore;

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
            // If this is a main view
            if(Enum.TryParse<MainViews>(value.ToString(), out MainViews mainView))
            {
                // Check the provided value
                switch (mainView)
                {
                    // Show a new instance of the serve interface
                    case MainViews.ServerInterface:
                        return new ServerView();

                    // Converter should never receive an invalid type
                    default:
                        throw new Exception("Page does not exist");
                }
            }

            // Return null if the provided value is invalid
            return null;
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
