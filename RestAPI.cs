using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using WooCommerceNET.Base;

namespace WooCommerceNET
{
    class DecimalConverter : JsonConverter
    {
        private static decimal Round(decimal x)
        {
            return Math.Round(x, 2);
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(decimal) || objectType == typeof(decimal?));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            bool isValueNull = token.Type == JTokenType.Null || (token.Type == JTokenType.String && string.IsNullOrEmpty(token.ToString()));

            if (isValueNull)
            {
                if (objectType == typeof(decimal?))
                {
                    return null;
                }
            }
            else
            {
                if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                {
                    return Round(token.ToObject<decimal>());
                }
                else if (token.Type == JTokenType.String)
                {
                    return Round(decimal.Parse(token.ToString(), CultureInfo.InvariantCulture));
                }
            }

            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null)
            {
                decimal val = (decimal)value;
                writer.WriteValue(val.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }

    public class RestAPI
    {
        private HttpClient httpClient;
        private JsonSerializer jsonSerializer;
        private string wc_url;
        private string wc_key;
        private string wc_secret;


        private bool AuthorizedHeader { get; set; }



        /// <summary>
        /// Initialize the RestAPI object
        /// </summary>
        /// <param name="url">WooCommerce REST API URL, e.g.: http://yourstore/wp-json/wc/v1/ </param>
        /// <param name="key">WooCommerce REST API Key</param>
        /// <param name="secret">WooCommerce REST API Secret</param>
        /// <param name="authorizedHeader">WHEN using HTTPS, do you prefer to send the Credentials in HTTP HEADER?</param>
        public RestAPI(string url, string key, string secret, bool authorizedHeader = true)
        {
            if (string.IsNullOrEmpty(url))
                throw new Exception("Please use a valid WooCommerce Restful API url.");

            url = url.Trim().TrimEnd('/');

            string urlLower = url.ToLower();
            if (urlLower.EndsWith("wc-api/v1") || urlLower.EndsWith("wc-api/v2") || urlLower.EndsWith("wc-api/v3"))
                Version = APIVersion.Legacy;
            else if (urlLower.EndsWith("wp-json/wc/v1"))
                Version = APIVersion.Version1;
            else if (urlLower.EndsWith("wp-json/wc/v2"))
                Version = APIVersion.Version2;
            else if (urlLower.Contains("wp-json/wc-"))
                Version = APIVersion.ThirdPartyPlugins;
            else
            {
                Version = APIVersion.Unknown;
                throw new Exception("Unknow WooCommerce Restful API version.");
            }

            wc_url = url + "/";
            wc_key = key;
            AuthorizedHeader = authorizedHeader;

            // Why extra '&'? look here: https://wordpress.org/support/topic/woocommerce-rest-api-v3-problem-woocommerce_api_authentication_error/
            if ((urlLower.EndsWith("wc-api/v3") || !IsLegacy) && !wc_url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                wc_secret = secret + "&";
            else
                wc_secret = secret;

            httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            });

            jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new DecimalConverter());
        }



        public bool IsLegacy
        {
            get
            {
                return Version == APIVersion.Legacy;
            }
        }

        public APIVersion Version { get; private set; }

        public string Url { get { return wc_url; } }

        /// <summary>
        /// Make Restful calls
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="endpoint"></param>
        /// <param name="method">HEAD, GET, POST, PUT, PATCH, DELETE</param>
        /// <param name="requestBody">If your call doesn't have a body, please pass string.Empty, not null.</param>
        /// <param name="parms"></param>
        /// <returns>json string</returns>
        public async Task<string> SendHttpClientRequest<T>(string endpoint, HttpMethod method, T requestBody, Dictionary<string, string> parms = null)
        {
            HttpRequestMessage httpRequest;

            if (wc_url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                if (AuthorizedHeader == true)
                {
                    httpRequest = new HttpRequestMessage(method, wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(wc_key + ":" + wc_secret)));
                }
                else
                {
                    if (parms == null)
                        parms = new Dictionary<string, string>();

                    if (!parms.ContainsKey("consumer_key"))
                        parms.Add("consumer_key", wc_key);
                    if (!parms.ContainsKey("consumer_secret"))
                        parms.Add("consumer_secret", wc_secret);

                    httpRequest = new HttpRequestMessage(method, wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                }
            }
            else
            {
                httpRequest = new HttpRequestMessage(method, wc_url + GetOAuthEndPoint(method.ToString(), endpoint, parms));
            }

            if (requestBody != null)
            {
                string bodyString = requestBody as string;

                if (!string.IsNullOrEmpty(bodyString))
                {
                    httpRequest.Content = new StringContent(bodyString, Encoding.UTF8, "application/json");
                }
                else if (bodyString is null)
                {
                    httpRequest.Content = new StringContent(SerializeJSon(requestBody), Encoding.UTF8, "application/json");
                }
            }

            try
            {
                HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest);
                string content = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode)
                {
                    return content;
                }
                else
                {
                    throw new WooCommerceException(httpResponse.StatusCode, content);
                }
            }
            catch (HttpRequestException ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetRestful(string endpoint, Dictionary<string, string> parms = null)
        {
            return await SendHttpClientRequest(endpoint, HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
        }

        public async Task<string> PostRestful(string endpoint, object jsonObject, Dictionary<string, string> parms = null)
        {
            return await SendHttpClientRequest(endpoint, HttpMethod.Post, jsonObject, parms).ConfigureAwait(false);
        }

        public async Task<string> PutRestful(string endpoint, object jsonObject, Dictionary<string, string> parms = null)
        {
            return await SendHttpClientRequest(endpoint, HttpMethod.Put, jsonObject, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteRestful(string endpoint, Dictionary<string, string> parms = null)
        {
            return await SendHttpClientRequest(endpoint, HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        private string GetOAuthEndPoint(string method, string endpoint, Dictionary<string, string> parms = null)
        {
            if (wc_url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                if (parms == null)
                    return endpoint;
                else
                {
                    string requestParms = string.Empty;
                    foreach (var parm in parms)
                        requestParms += parm.Key + "=" + parm.Value + "&";

                    return endpoint + "?" + requestParms.TrimEnd('&');
                }
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("oauth_consumer_key", wc_key);
            dic.Add("oauth_nonce", Guid.NewGuid().ToString("N"));
            dic.Add("oauth_signature_method", "HMAC-SHA256");
            dic.Add("oauth_timestamp", Common.GetUnixTime(false));

            if (parms != null)
                foreach (var p in parms)
                    dic.Add(p.Key, p.Value);

            string base_request_uri = method.ToUpper() + "&" + Uri.EscapeDataString(wc_url + endpoint) + "&";
            string stringToSign = string.Empty;

            foreach (var parm in dic.OrderBy(x => x.Key))
                stringToSign += Uri.EscapeDataString(parm.Key) + "=" + Uri.EscapeDataString(parm.Value) + "&";

            base_request_uri = base_request_uri + Uri.EscapeDataString(stringToSign.TrimEnd('&'));

            dic.Add("oauth_signature", Common.GetSHA256(wc_secret, base_request_uri));

            string parmstr = string.Empty;
            foreach (var parm in dic)
                parmstr += parm.Key + "=" + Uri.EscapeDataString(parm.Value) + "&";

            return endpoint + "?" + parmstr.TrimEnd('&');
        }

        public virtual string SerializeJSon<T>(T t)
        {
            var buffer = new StringWriter();
            jsonSerializer.Serialize(buffer, t);
            return buffer.ToString();
        }

        public virtual T DeserializeJSon<T>(string jsonString)
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(jsonString));
            return jsonSerializer.Deserialize<T>(reader);
        }

        public string DateTimeFormat
        {
            get
            {
                return IsLegacy ? "yyyy-MM-ddTHH:mm:ssZ" : "yyyy-MM-ddTHH:mm:ss";
            }
        }
    }

    public enum APIVersion
    {
        Unknown = 0,
        Legacy = 1,
        Version1 = 2,
        Version2 = 3,
        ThirdPartyPlugins = 99
    }
}
