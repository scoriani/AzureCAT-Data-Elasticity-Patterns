using System;

namespace Microsoft.AzureCat.Patterns.DataElasticity.Client
{
    /// <summary>
    /// Class ElasticDataException is thrown by the Elastic Data system.
    /// </summary>
    public class ElasticDataException : ApplicationException
    {
        #region constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticDataException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">A message that describes the error.</param>
        public ElasticDataException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ElasticDataException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public ElasticDataException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        #endregion
    }
}