namespace NetworkModules.Server
{
    using System;
    using System.Net.Sockets;

    /// <summary>
    /// Exception for when an invalid value was provided
    /// </summary>
    public class InvalidValueException : Exception
    {
        /// <summary>
        /// Default constructor that calls the base constructor
        /// </summary>
        /// <param name="prop">Property name</param>
        /// <param name="value">Value that was given</param>
        public InvalidValueException(string prop, object value) : 
            base($"[{value}] is not a valid value for [{prop}]") { }

    }

    /// <summary>
    /// Exception for when a client fails to verify
    /// </summary>
    public class InvalidHandshakeException : Exception
    {
        /// <summary>
        /// Default constructor that calls the base constructor
        /// </summary>
        /// <param name="s"></param>
        public InvalidHandshakeException(Socket s) :
            base(s.RemoteEndPoint.ToString()) { }

    }
}
