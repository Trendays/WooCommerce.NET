using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerce.NET
{
    public class WooCommerceException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string ErrorMessage { get; private set; }

        public WooCommerceException(HttpStatusCode statusCode, string errorMessage, Exception ex) : base(errorMessage, ex)
        {
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
        }
    }
}
