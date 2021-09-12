/// <summary>
/// Root namespace
/// </summary>
namespace NetworkModules.Core
{
    // Required namespaces
    using System;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// 
    /// </summary>
    public abstract class MessagesHandler
    {
        #region Private properties

        /// <summary>
        /// Gets the message file path.
        /// </summary>
        public abstract string MessageFilePath { get; }

        #endregion

        #region Private functions

        /// <summary>
        /// Gets the row with the specified id.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        private string GetRow(object id, params object[] parameters)
        {
            // Get the correct row in the message file
            string row = File.ReadAllLines(MessageFilePath)
                .Where(r => !r.StartsWith(';'))
                .FirstOrDefault(r => r.Split(':')[0].CompareTo(id.ToString()) == 0);

            // Throw exception if no row was found
            if (row == null) throw new Exception();

            // Loop through all parameters
            for (int i = 0; i < parameters.Length; i++)
            {
                // If the parameter is not null...
                if(parameters[i] != null)
                    // insert the parameter
                    row = row.Replace("{" + i + "}", parameters[i].ToString());
            }

            // return the message string
            return row[(row.Split(':')[0].Length + 1)..];
        }

        #endregion

        #region Public functions

        /// <summary>
        /// Gets the specified error message.
        /// </summary>
        /// <param name="errID">The error message identifier.</param>
        /// <returns></returns>
        public string GetErrorMessage(object errID, params object[] parameters)
        {
            // Try getting the error message
            try
            {
                // Return the error message
                return GetRow(errID, parameters);
            }
            // If the message file could not be found
            catch(DirectoryNotFoundException)           
            {
                // Return error id only
                return $"[Error {errID} could not be retrieved]";
            }
            catch (FileNotFoundException)
            {
                // Return error id only
                return $"[Error {errID} could not be retrieved]";
            }
            // If a message for the provided ID could not be found
            catch
            {
                // Return undefined error
                return $"[Undefined error - {errID}]";
            }
        }

        /// <summary>
        /// Gets the specified information message.
        /// </summary>
        /// <param name="messageID">The message identifier.</param>
        /// <returns></returns>
        public string GetMessage(object messageID, params object[] parameters)
        {
            // Try getting the message
            try
            {
                return GetRow(messageID, parameters);
            }
            // If the message file could not be found
            catch (DirectoryNotFoundException)
            {
                // Return message id only
                return $"[Message {messageID} could not be retrieved]";
            }
            // If the message file could not be found
            catch (FileNotFoundException)
            {
                // Return message id only
                return $"[Message {messageID} could not be retrieved]";
            }
            // If a message for the provided ID could not be found
            catch
            {
                // Return undefined message
                return $"[Undefined message - {messageID}]";
            }
        }

        /// <summary>
        /// Gets the specified information message.
        /// </summary>
        /// <param name="messageID">The message identifier.</param>
        /// <returns></returns>
        public string GetWarning(object warningMSGID, params object[] parameters)
        {
            // Try getting the warning message
            try
            {
                // Return the warning message
                return GetRow(warningMSGID, parameters);
            }
            // If the message file could not be found
            catch (DirectoryNotFoundException)
            {
                // Return warning message id only
                return $"[Warning message {warningMSGID} could not be retrieved]";
            }
            // If the message file could not be found
            catch (FileNotFoundException)
            {
                // Return warning message id only
                return $"[Warning message {warningMSGID} could not be retrieved]";
            }
            // If a message for the provided ID could not be found
            catch
            {
                // Return undefined warning
                return $"[Undefined warning - {warningMSGID}]";
            }
        }

        #endregion
    }
}
