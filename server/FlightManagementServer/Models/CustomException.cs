using System.Runtime.Serialization;

namespace FlightManagementServer.Models
{
    public class CustomException : Exception
    {
        public CustomException()
            : base() { }

        public CustomException(string message)
            : base(message) { }

        public CustomException(string message, Exception innerException)
            : base(message, innerException) { }

        protected CustomException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}