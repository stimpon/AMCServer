namespace AMCClient2
{
    /// <summary>
    /// Required namespaces
    /// </summary>
    #region Namespaces
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Markup;
    #endregion

    /// <summary>
    /// Base converter for all other converters
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseValueConverter<T> : MarkupExtension, IValueConverter
        where T : class, new()
    {
        /// <summary>
        /// Instance of this converter
        /// </summary>
        private T ThisConverter { get; set; }

        /// <summary>
        /// Convert the provided value
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        /// <summary>
        /// Convert back into its root object
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

        /// <summary>
        /// Provide a instance of this class for the view
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // Return a new instance of this converter if the instance is null
            return ThisConverter ?? new T();
        }
    }
}
