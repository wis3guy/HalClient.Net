using System;
using System.Net;
using System.Runtime.Serialization;
using HalClient.Net.Parser;

namespace HalClient.Net
{
    [Serializable]
    public class HalHttpRequestException : Exception
    {
#if REQUIRES_EXCEPTION_SERIALIZATION_CTOR
        protected HalHttpRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif

        public HalHttpRequestException(HttpStatusCode statusCode, string reason, IRootResourceObject resource = null)
            : base($"{(int)statusCode} ({reason})")
        {
            StatusCode = statusCode;
            Resource = resource;
        }

        public HttpStatusCode StatusCode { get; }
        public IRootResourceObject Resource { get; }
    }
}