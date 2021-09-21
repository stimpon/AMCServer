namespace NetworkModules.Core
{
    // Requiread namespaces
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Abstract class for message reading
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MessageReader<T> where T : IMessage, new()
    {
        /// <summary>
        /// Gets the specified message
        /// </summary>
        /// <param name="code">The message code.</param>
        /// <param name="parameters">The parameters for the message.</param>
        /// <returns><see cref="T"/></returns>
        public static T Get(int code, params object[] parameters)
        {
            // Create the result message
            var msg = new T();

            // Open up a reader for reading the socket exception file
            using (StreamReader sr = new StreamReader(msg.MessageSourceFile))
            {
                // Declare placeholder for current line
                string currLine = String.Empty;

                // Will be used to check if we have found the message we are looking for
                bool errFound = false;
                // Will be used to check which part of the message we are building right now
                int msgBuildingState = -1;
                // Messages can be long, so we use a builder so we can append unlimited lines
                var msgBuilder = new StringBuilder();

                // While not at the end of the file
                while ((currLine = sr.ReadLine()) != null)
                {
                    // If the message has been red
                    if (errFound && String.IsNullOrEmpty(currLine))
                    {
                        // Don't read any more from the exception file
                        break;
                    }

                    // Try to parse the err code
                    if (int.TryParse(currLine, out int errCode) && errCode == code)
                    {
                        // We have found our message
                        errFound = true;

                        msgBuildingState++;
                    }

                    switch (msgBuildingState)
                    {
                        // Set error code of the exception
                        case 0:
                            // Set the error code
                            msg.Code = errCode;
                            // Go to the next stage
                            msgBuildingState++;
                            break;

                        // Set the title of the exception
                        case 1:
                            // Set the error title
                            msg.Title = currLine;
                            // Go to the next stage
                            msgBuildingState++;
                            break;

                        // Set the error message
                        case 2:
                            msgBuilder.AppendLine(currLine);
                            break;

                        default: break;
                    }
                }

                // Set the message
                msg.Message = msgBuilder.ToString().TrimEnd();

                // If we have any parameters for the message
                if (parameters.Length > 0 && !String.IsNullOrEmpty(msg.Message))
                {
                    // Loop through all parameters
                    for(int i = 0; i < parameters.Length; i++)
                    {
                        // If the parameter is not missing
                        if (parameters[i] != null)
                        {
                            // Insert the parameter
                            msg.Message = msg.Message.Replace("{" + i + "}", parameters[i].ToString());
                        }
                    }
                }

            }

            // Return the message
            return msg;
        }
    }
}
