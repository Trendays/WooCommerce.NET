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

        public APIVersion Version { get; private set; }
        public string BaseUrl { get; private set; }
        public string ClientKey { get; private set; }
        public string ClientSecret { get; private set; }
        public bool AuthorizedHeader { get; private set; }

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
            if (urlLower.EndsWith("wp-json/wc/v1"))
                Version = APIVersion.Version1;
            else if (urlLower.EndsWith("wp-json/wc/v2"))
                Version = APIVersion.Version2;
            else if (urlLower.Contains("wp-json/wc-"))
                Version = APIVersion.ThirdPartyPlugins;
            else
            {
                throw new Exception("Unknow WooCommerce Restful API version.");
            }

            BaseUrl = url + "/";
            ClientKey = key;
            AuthorizedHeader = authorizedHeader;

            // Why extra '&'? look here: https://wordpress.org/support/topic/woocommerce-rest-api-v3-problem-woocommerce_api_authentication_error/
            if (!BaseUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ClientSecret = secret + "&";
            }
            else
            {
                ClientSecret = secret;
            }

            httpClient = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            });

            jsonSerializer = new JsonSerializer();
            jsonSerializer.Converters.Add(new DecimalConverter());
        }

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

            if (BaseUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                if (AuthorizedHeader == true)
                {
                    httpRequest = new HttpRequestMessage(method, BaseUrl + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                    httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{ClientKey}:{ClientSecret}")));
                }
                else
                {
                    if (parms == null)
                    {
                        parms = new Dictionary<string, string>();
                    }

                    if (!parms.ContainsKey("consumer_key"))
                    {
                        parms.Add("consumer_key", ClientKey);
                    }

                    if (!parms.ContainsKey("consumer_secret"))
                    {
                        parms.Add("consumer_secret", ClientSecret);
                    }

                    httpRequest = new HttpRequestMessage(method, BaseUrl + GetOAuthEndPoint(method.ToString(), endpoint, parms));
                }
            }
            else
            {
                httpRequest = new HttpRequestMessage(method, BaseUrl + GetOAuthEndPoint(method.ToString(), endpoint, parms));
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
            // OAuth 1.0 spec: https://tools.ietf.org/html/rfc5849#section-3.1

            if (BaseUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                if (parms == null)
                {
                    return endpoint;
                }
                else
                {
                    return $"{endpoint}?{BuildQueryString(parms)}";
                }
            }

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("oauth_consumer_key", ClientKey);
            dic.Add("oauth_nonce", Guid.NewGuid().ToString("N"));
            dic.Add("oauth_signature_method", "HMAC-SHA256");
            dic.Add("oauth_timestamp", Common.GetUnixTime(false));

            if (parms != null)
            {
                foreach (var p in parms)
                {
                    dic.Add(p.Key, p.Value);
                }
            }

            string stringToSign = BuildQueryString(dic.OrderBy(x => x.Key));
            string oauthUri = $"{method.ToUpper()}&{EscapeComponent(BaseUrl + endpoint)}&{EscapeComponent(stringToSign)}";

            dic.Add("oauth_signature", Common.GetSHA256(ClientSecret, oauthUri));

            return $"{endpoint}?{BuildQueryString(dic)}";
        }

        private string EscapeComponent(string val)
        {
            return Uri.EscapeDataString(val);
        }

        private string BuildQueryString(IEnumerable<KeyValuePair<string, string>> parms)
        {
            return string.Join("&", parms.Select(p => $"{EscapeComponent(p.Key)}={EscapeComponent(p.Value)}"));
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
    }

    public enum APIVersion
    {
        Version1 = 2,
        Version2 = 3,
        ThirdPartyPlugins = 99
    }
}
