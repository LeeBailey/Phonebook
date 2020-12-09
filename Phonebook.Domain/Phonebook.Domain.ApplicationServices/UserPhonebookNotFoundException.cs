using System;
using System.Runtime.Serialization;

namespace Phonebook.Domain.ApplicationServices
{
    /// <summary>
    /// This exception is thrown if a user's phonebook that was expected to exist could not be found.
    /// </summary>
    [Serializable]
    public class UserPhonebookNotFoundException : Exception
    {
        /// <summary>
        /// Id of the User.
        /// </summary>
        public object? UserId { get; set; }

        /// <summary>
        /// Creates a new <see cref="UserPhonebookNotFoundException"/> object.
        /// </summary>
        public UserPhonebookNotFoundException()
        {
        }

        /// <summary>
        /// Creates a new <see cref="UserPhonebookNotFoundException"/> object.
        /// </summary>
        public UserPhonebookNotFoundException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {
        }

        /// <summary>
        /// Creates a new <see cref="UserPhonebookNotFoundException"/> object.
        /// </summary>
        public UserPhonebookNotFoundException(object userId)
            : this(userId, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="UserPhonebookNotFoundException"/> object.
        /// </summary>
        public UserPhonebookNotFoundException(object userId, Exception? innerException)
            : base($"A phonebook with the following userId could not be found: {userId}", innerException)
        {
            UserId = userId;
        }

        /// <summary>
        /// Creates a new <see cref="UserPhonebookNotFoundException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public UserPhonebookNotFoundException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Creates a new <see cref="UserPhonebookNotFoundException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public UserPhonebookNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
