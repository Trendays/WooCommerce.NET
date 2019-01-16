using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WooCommerceNET.Base
{
    [DataContract]
    public class JsonObject
    {
    }

    public class BatchObject<T>
    {
        [DataMember(EmitDefaultValue = false)]
        public List<T> create { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<T> update { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<int> delete { get; set; }
    }

    public class WCItem<T>
    {
        public string APIEndpoint { get; protected set; }
        public RestAPI API { get; protected set; }

        public WCItem(RestAPI api)
        {
            API = api;
            if(typeof(T).BaseType.GetRuntimeProperty("Endpoint") == null)
                APIEndpoint = typeof(T).GetRuntimeProperty("Endpoint").GetValue(null).ToString();
            else
                APIEndpoint = typeof(T).BaseType.GetRuntimeProperty("Endpoint").GetValue(null).ToString();
        }

        public async Task<T> Get(int id, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<T>(await API.GetRestful(APIEndpoint + "/" + id.ToString(), parms).ConfigureAwait(false));
        }

        public async Task<T> Get(string email, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<T>(await API.GetRestful(APIEndpoint + "/" + email, parms).ConfigureAwait(false));
        }

        public async Task<List<T>> GetAll(Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<List<T>>(await API.GetRestful(APIEndpoint, parms).ConfigureAwait(false));
        }

        public async Task<T> Add(T item, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<T>(await API.PostRestful(APIEndpoint, item, parms).ConfigureAwait(false));
        }

        public async Task<BatchObject<T>> AddRange(BatchObject<T> items, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<BatchObject<T>>(await API.PostRestful(APIEndpoint + "/batch", items, parms).ConfigureAwait(false));
        }

        public async Task<T> Update(int id, T item, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<T>(await API.PostRestful(APIEndpoint + "/" + id.ToString(), item, parms).ConfigureAwait(false));
        }

        public async Task<T> UpdateWithNull(int id, object item, Dictionary<string, string> parms = null)
        {
            if (API.GetType().Name == "RestAPI")
            {
                StringBuilder json = new StringBuilder();
                json.Append("{");
                foreach(var prop in item.GetType().GetRuntimeProperties())
                {
                    json.Append($"\"{prop.Name}\": \"\", ");
                }

                if (json.Length > 1)
                    json.Remove(json.Length - 2, 1);

                json.Append("}");

                return API.DeserializeJSon<T>(await API.PostRestful(APIEndpoint + "/" + id.ToString(), json.ToString(), parms).ConfigureAwait(false));
            }
            else
                return API.DeserializeJSon<T>(await API.PostRestful(APIEndpoint + "/" + id.ToString(), item, parms).ConfigureAwait(false));
        }

        public async Task<BatchObject<T>> UpdateRange(BatchObject<T> items, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<BatchObject<T>>(await API.PostRestful(APIEndpoint + "/batch", items, parms).ConfigureAwait(false));
        }

        public async Task<string> Delete(int id, bool force = false, Dictionary<string, string> parms = null)
        {
            if (force)
            {
                if (parms == null)
                    parms = new Dictionary<string, string>();

                if (!parms.ContainsKey("force"))
                    parms.Add("force", "true");
            }

            return await API.DeleteRestful(APIEndpoint + "/" + id.ToString(), parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteRange(BatchObject<T> items, Dictionary<string, string> parms = null)
        {
            return await API.PostRestful(APIEndpoint + "/batch", items, parms).ConfigureAwait(false);
        }
    }

    public class WCSubItem<T>
    {
        public string APIEndpoint { get; protected set; }
        public string APIParentEndpoint { get; protected set; }
        public RestAPI API { get; protected set; }

        public WCSubItem(RestAPI api, string parentEndpoint)
        {
            API = api;
            APIEndpoint = typeof(T).GetRuntimeProperty("Endpoint").GetValue(null).ToString();
            APIParentEndpoint = parentEndpoint;
        }

        public async Task<T> Get(int id, int parentId, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<T>(await API.GetRestful(APIParentEndpoint + "/" + parentId.ToString() + "/" + APIEndpoint + "/" + id.ToString(), parms).ConfigureAwait(false));
        }

        public async Task<List<T>> GetAll(object parentId, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<List<T>>(await API.GetRestful(APIParentEndpoint + "/" + parentId.ToString() + "/" + APIEndpoint, parms).ConfigureAwait(false));
        }

        public async Task<T> Add(T item, int parentId, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<T>(await API.PostRestful(APIParentEndpoint + "/" + parentId.ToString() + "/" + APIEndpoint, item, parms).ConfigureAwait(false));
        }

        public async Task<T> Update(int id, T item, int parentId, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<T>(await API.PostRestful(APIParentEndpoint + "/" + parentId.ToString() + "/" + APIEndpoint + "/" + id.ToString(), item, parms).ConfigureAwait(false));
        }

        public async Task<BatchObject<T>> UpdateRange(int parentId, BatchObject<T> items, Dictionary<string, string> parms = null)
        {
            return API.DeserializeJSon<BatchObject<T>>(await API.PostRestful(APIParentEndpoint + "/" + parentId.ToString() + "/" + APIEndpoint + "/batch", items, parms).ConfigureAwait(false));
        }

        public async Task<string> Delete(int id, int parentId, bool force = false, Dictionary<string, string> parms = null)
        {
            return await API.DeleteRestful(APIParentEndpoint + "/" + parentId.ToString() + "/" + APIEndpoint + "/" + id.ToString(), parms).ConfigureAwait(false);
        }
    }
}
