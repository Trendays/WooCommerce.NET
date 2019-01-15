using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerceNET
{
    public class WooCommerceException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string ErrorMessage { get; private set; }

        public WooCommerceException(HttpStatusCode statusCode, string errorMessage) : base($"WooCommerceException: {statusCode.ToString()}")
        {
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
        }
    }
}
