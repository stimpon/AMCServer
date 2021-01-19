namespace AMCCore
{
    /// <summary>
    /// Class that contains formating functions
    /// </summary>
    public static class StringFormatingHelpers
    {
        /// <summary>
        /// Formats bytes to a representable string
        /// </summary>
        /// <param name="Size"></param>
        /// <returns></returns>
        public static string BytesToSizeString(long Size)
        {
            // Checks byte value
            if (Size <= 1000)
                return Size.ToString() + " b";
            else if (Size <= 500000)
                return ((float)Size / 1000).ToString("0.0") + " kb";
            else if (Size <= 1000000000)
                return ((float)Size / 1000000).ToString("0.00") + " mb";
            else if (Size <= 1000000000000)
                return ((float)Size / 1000000000).ToString("0.00") + " gb";

            return string.Empty;
        }
        /// <summary>
        /// Formats bytes to a representable string
        /// </summary>
        /// <param name="Size"></param>
        /// <returns></returns>
        public static string BytesToSizeString(double Size)
        {
            // Checks byte value
            if (Size <= 1000)
                return Size.ToString() + " b";
            else if (Size <= 500000)
                return ((float)Size / 1000).ToString("0.0") + " kb";
            else if (Size <= 1000000000)
                return ((float)Size / 1000000).ToString("0.00") + " mb";
            else if (Size <= 1000000000000)
                return ((float)Size / 1000000000).ToString("0.00") + " gb";

            return string.Empty;
        }
        /// <summary>
        /// Formats bytes to a representable string
        /// </summary>
        /// <param name="Size"></param>
        /// <returns></returns>
        public static string BytesToSizeString(decimal Size)
        {
            // Checks byte value
            if (Size <= 1000)
                return Size.ToString() + " b";
            else if (Size <= 500000)
                return ((float)Size / 1000).ToString("0.0") + " kb";
            else if (Size <= 1000000000)
                return ((float)Size / 1000000).ToString("0.00") + " mb";
            else if (Size <= 1000000000000)
                return ((float)Size / 1000000000).ToString("0.00") + " gb";

            return string.Empty;
        }

    }
}
