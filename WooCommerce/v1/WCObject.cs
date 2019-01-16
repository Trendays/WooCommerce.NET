using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WooCommerceNET.Base;

namespace WooCommerceNET.WooCommerce.v1
{
    /// <summary>
    /// Base WoCommerce object
    /// </summary>
    /// <typeparam name="T1">Coupon</typeparam>
    /// <typeparam name="T2">Customer</typeparam>
    /// <typeparam name="T3">Order</typeparam>
    /// <typeparam name="T4">OrderNote</typeparam>
    /// <typeparam name="T5">OrderRefund</typeparam>
    /// <typeparam name="T6">Product</typeparam>
    /// <typeparam name="T7">ProductCategory</typeparam>
    /// <typeparam name="T8">ProductAttribute</typeparam>
    /// <typeparam name="T9">ProductAttributeTerm</typeparam>
    /// <typeparam name="T10">ShippingClass</typeparam>
    /// <typeparam name="T11">ProductTag</typeparam>
    /// <typeparam name="T12">TaxRate</typeparam>
    /// <typeparam name="T13">TaxClass</typeparam>
    /// <typeparam name="T14">Webhook</typeparam>
    public class WCObject<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>
        where T1 : Coupon where T2 : Customer where T3 : Order where T4 : OrderNote where T5 : OrderRefund
        where T6 : Product where T7 : ProductCategory where T8 : ProductAttribute where T9 : ProductAttributeTerm
        where T10 : ShippingClass where T11 : ProductTag where T12 : TaxRate where T13 : TaxClass where T14 : Webhook
    {
        protected RestAPI API;
        public WCObject(RestAPI api)
        {
            if (api.Version != APIVersion.Version1)
                throw new Exception("Please use WooCommerce Restful API Version 1 url for this WCObject. e.g.: http://www.yourstore.co.nz/wp-json/wc/v1/");

            API = api;
        }

        public async Task<string> GetStoreInfo()
        {
            return await API.SendHttpClientRequest(string.Empty, HttpMethod.Get, string.Empty).ConfigureAwait(false);
        }

        #region "coupons..."

        public async Task<List<T1>> GetCoupons(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("coupons", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T1>>(json);
        }

        public async Task<T1> GetCoupon(int couponid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("coupons/" + couponid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T1>(json);
        }

        public async Task<T1> GetCoupon(string code, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("coupons/code/" + code, HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T1>(json);
        }

        public async Task<string> PostCoupon(T1 c, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("coupons", HttpMethod.Post, c, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateCoupon(int couponid, T1 c, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("coupons/" + couponid.ToString(), HttpMethod.Put, c, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateCoupons(BatchObject<T1> cb, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("coupons/batch", HttpMethod.Put, cb, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteCoupon(int couponid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("coupons/" + couponid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        #endregion

        #region "customers..."

        public async Task<List<T2>> GetCustomers(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("customers", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T2>>(json);
        }

        public async Task<T2> GetCustomer(int id, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("customers/" + id.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T2>(json);
        }

        public async Task<T2> GetCustomerByEmail(string email, Dictionary<string, string> parms = null)
        {
            if (parms == null)
                parms = new Dictionary<string, string>();

            parms.Add("email", email);

            string json = await API.SendHttpClientRequest("customers", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);

            List<T2> c = API.DeserializeJSon<List<T2>>(json);
            if (c.Count == 0)
                return null;
            else
                return c[0];
        }

        [Obsolete("GetCustomerOrders is deprecated in v1, please use GetOrders method with customer parameter instead.", true)]
        public async Task<List<T3>> GetCustomerOrders(int id, Dictionary<string, string> parms = null)
        {
            string json = await API.GetRestful("customers/" + id.ToString() + "/orders", parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T3>>(json);
        }

        public async Task<List<Download>> GetCustomerDownloads(int id, Dictionary<string, string> parms = null)
        {
            string json = await API.GetRestful("customers/" + id.ToString() + "/downloads", parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<Download>>(json);
        }

        //Don't forget to include a password when creating a customer, the example in REST API DOCS will not work!!!
        public async Task<string> PostCustomer(T2 c, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("customers", HttpMethod.Post, c, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateCustomer(int id, T2 c, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("customers/" + id.ToString(), HttpMethod.Post, c, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateCustomers(BatchObject<T2> cb, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("customers/batch", HttpMethod.Post, cb, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteCustomer(int id, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("customers/" + id.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        #endregion

        #region "orders..."

        public async Task<List<T3>> GetOrders(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("orders", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T3>>(json);
        }

        public async Task<T3> GetOrder(int orderid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("orders/" + orderid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T3>(json);
        }

        public async Task<List<KeyValuePair<string, string>>> GetOrderStatuses(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("orders/statuses", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            json = json.Substring(20, json.Length - 22).Replace("\"", string.Empty);

            List<KeyValuePair<string, string>> statuses = new List<KeyValuePair<string, string>>();
            foreach (string status in json.Split(','))
            {
                KeyValuePair<string, string> value = new KeyValuePair<string, string>(status.Split(':')[0], status.Split(':')[1]);
                statuses.Add(value);
            }

            return statuses;
        }

        public async Task<string> PostOrder(T3 c, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("orders", HttpMethod.Post, c, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateOrder(int id, T3 c, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("orders/" + id.ToString(), HttpMethod.Put, c, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateOrders(BatchObject<T3> ob, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("orders/batch", HttpMethod.Put, ob, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteOrder(int orderid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("orders/" + orderid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        #endregion

        #region "Order notes..."

        public async Task<List<T4>> GetOrderNotes(int id, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("orders/" + id.ToString() + "/notes", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T4>>(json);
        }

        public async Task<T4> GetOrderNote(int orderid, int noteid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("orders/" + orderid.ToString() + "/notes/" + noteid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T4>(json);
        }

        public async Task<string> PostOrderNote(int orderid, T4 n, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("orders/" + orderid.ToString() + "/notes", HttpMethod.Post, n, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateOrderNote(int orderid, int noteid, T4 n, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("orders/" + orderid.ToString() + "/notes/" + noteid.ToString(), HttpMethod.Put, n, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteOrderNote(int orderid, int noteid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("orders/" + orderid.ToString() + "/notes/" + noteid.ToString(), HttpMethod.Delete, parms).ConfigureAwait(false);
        }

        #endregion

        #region "Order refunds..."

        public async Task<List<T5>> GetOrderRefunds(int orderid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("orders/" + orderid.ToString() + "/refunds", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T5>>(json);
        }

        public async Task<T5> GetOrderRefund(int orderid, int refundid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("orders/" + orderid.ToString() + "/refunds/" + refundid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T5>(json);
        }

        public async Task<string> PostOrderRefund(int orderid, T5 r, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("orders/" + orderid.ToString() + "/refunds", HttpMethod.Post, r, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateOrderRefund(int orderid, int refundid, T5 r, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("orders/" + orderid.ToString() + "/refunds/" + refundid.ToString(), HttpMethod.Put, r, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteOrderRefund(int orderid, int refundid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("orders/" + orderid.ToString() + "/refunds/" + refundid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }


        #endregion

        #region "products..."

        public async Task<List<T6>> GetProducts(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T6>>(json);
        }

        public async Task<T6> GetProduct(int productid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/" + productid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T6>(json);
        }

        public async Task<string> PostProduct(T6 p, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products", HttpMethod.Post, p, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProduct(int productid, T6 p, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/" + productid.ToString(), HttpMethod.Put, p, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProducts(BatchObject<T6> pb, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/batch", HttpMethod.Put, pb, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteProduct(int productid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/" + productid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        public async Task<List<Variation>> GetProductVariations(int productId, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest($"products/{productId.ToString()}/variations", HttpMethod.Get, string.Empty, parms);
            return API.DeserializeJSon<List<Variation>>(json);
        }

        #endregion

        #region "Product reviews..."

        public async Task<List<ProductReview>> GetProductReviews(int productid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/" + productid.ToString() + "/reviews", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<ProductReview>>(json);
        }

        #endregion

        #region "Product categories..."

        public async Task<List<T7>> GetProductCategories(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/categories", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T7>>(json);
        }

        public async Task<T7> GetProductCategory(int categoryid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/categories/" + categoryid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T7>(json);
        }

        public async Task<string> PostProductCategory(T7 pc, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/categories", HttpMethod.Post, pc, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProductCategory(int categoryid, T7 pc, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/categories/" + categoryid.ToString(), HttpMethod.Put, pc, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProductCategories(BatchObject<T7> cb, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/categories/batch", HttpMethod.Put, cb, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteProductCategory(int categoryid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/categories/" + categoryid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        #endregion

        #region "Product attributes..."

        public async Task<List<T8>> GetProductAttributes(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/attributes", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T8>>(json);
        }

        public async Task<T8> GetProductAttribute(int attributeid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/attributes/" + attributeid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T8>(json);
        }

        public async Task<string> PostProductAttribute(T8 pa, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/attributes", HttpMethod.Post, pa, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProductAttribute(int attributeid, T8 pa, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/attributes/" + attributeid.ToString(), HttpMethod.Put, pa, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProductAttributes(BatchObject<T8> ab, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/attributes/batch", HttpMethod.Put, ab, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteProductAttribute(int attributeid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/attributes/" + attributeid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        #endregion

        #region "Product Attribute terms..."

        public async Task<List<T9>> GetProductAttributeTerms(int attributeid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/attributes/" + attributeid.ToString() + "/terms", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T9>>(json);
        }

        public async Task<T9> GetProductAttributeTerm(int attributeid, int termid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/attributes/" + attributeid.ToString() + "/terms/" + termid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T9>(json);
        }

        public async Task<string> PostProductAttributeTerm(int attributeid, T9 t, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/attributes/" + attributeid.ToString() + "/terms", HttpMethod.Post, t, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProductAttributeTerm(int attributeid, int termid, T9 t, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/attributes/" + attributeid.ToString() + "/terms/" + termid.ToString(), HttpMethod.Put, t, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteProductAttributeTerm(int attributeid, int termid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/attributes/" + attributeid.ToString() + "/terms/" + termid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }


        #endregion

        #region "Product Shipping class..."

        public async Task<List<T10>> GetProductShippingClasses(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/shipping_classes", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T10>>(json);
        }

        public async Task<T10> GetProductShippingClass(int shippingclassid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/shipping_classes/" + shippingclassid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T10>(json);
        }

        public async Task<string> PostProductShippingClass(T10 sc, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/shipping_classes", HttpMethod.Post, sc, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProductShippingClass(int shippingclassid, T10 sc, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/shipping_classes/" + shippingclassid.ToString(), HttpMethod.Put, sc, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProductShippingClasses(BatchObject<T10> sb, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/shipping_classes/batch", HttpMethod.Put, sb, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteProductShippingClass(int shippingclassid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/shipping_classes/" + shippingclassid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        #endregion

        #region "Product tags..."

        public async Task<List<T11>> GetProductTags(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/tags", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T11>>(json);
        }

        public async Task<T11> GetProductTag(int tagid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("products/tags/" + tagid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T11>(json);
        }

        public async Task<string> PostProductTag(T11 pt, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/tags", HttpMethod.Post, pt, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProductTag(int tagid, T11 pt, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/tags/" + tagid.ToString(), HttpMethod.Put, pt, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateProductTags(BatchObject<T11> pb, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/tags/batch", HttpMethod.Put, pb, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteProductTag(int tagid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("products/tags/" + tagid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        #endregion

        #region "Tax rates..."

        public async Task<List<T12>> GetTaxRates(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("taxes", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T12>>(json);
        }

        public async Task<T12> GetTaxRate(int taxRateid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("taxes/" + taxRateid.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T12>(json);
        }

        public async Task<string> PostTaxRate(T12 t, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("taxes", HttpMethod.Post, t, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateTaxRate(int taxrateid, T12 t, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("taxes/" + taxrateid.ToString(), HttpMethod.Put, t, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateTaxRates(BatchObject<T12> tb, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("taxes/batch", HttpMethod.Put, tb, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteTaxRate(int taxrateid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("taxes/" + taxrateid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        #endregion

        #region "Tax classes..."

        public async Task<List<T13>> GetTaxClasses(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("taxes/classes", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T13>>(json);
        }

        public async Task<string> PostTaxClass(T13 t, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("taxes/classes", HttpMethod.Post, t, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteTaxClass(string slug, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("taxes/" + slug, HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        #endregion

        #region "reports..."

        public async Task<List<Report>> GetReports(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("reports", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<Report>>(json);
        }

        public async Task<List<SalesReport>> GetSalesReport(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("reports/sales", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<SalesReport>>(json);
        }

        public async Task<TopSellersReport> GetTopSellerReport(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("reports/sales/top_sellers", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<TopSellersReport>(json);
        }

        #endregion

        #region "webhooks..."

        public async Task<List<T14>> GetWebhooks(Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("webhooks", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T14>>(json);
        }

        public async Task<T14> GetWebhook(int id, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("webhooks/" + id.ToString(), HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<T14>(json);
        }

        public async Task<string> PostWebhook(T14 obj, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("webhooks", HttpMethod.Post, obj, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateWebhook(int webhookid, T14 obj, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("webhooks/" + webhookid.ToString(), HttpMethod.Put, obj, parms).ConfigureAwait(false);
        }

        public async Task<string> UpdateWebhooks(BatchObject<T14> obj, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("webhooks/batch", HttpMethod.Put, obj, parms).ConfigureAwait(false);
        }

        public async Task<string> DeleteWebhook(int webhookid, Dictionary<string, string> parms = null)
        {
            return await API.SendHttpClientRequest("webhooks/" + webhookid.ToString(), HttpMethod.Delete, string.Empty, parms).ConfigureAwait(false);
        }

        public async Task<List<T14>> GetWebhookDeliveries(int webhookid, Dictionary<string, string> parms = null)
        {
            string json = await API.SendHttpClientRequest("webhooks/" + webhookid.ToString() + "/deliveries", HttpMethod.Get, string.Empty, parms).ConfigureAwait(false);
            return API.DeserializeJSon<List<T14>>(json);
        }

        public async Task<WebhookDelivery> GetWebhookDelivery(int webhookid, int deliveryid, Dictionary<string, string> parms = null)
        {
            string json = await API.GetRestful("webhooks/" + webhookid.ToString() + "/deliveries/" + deliveryid.ToString(), parms).ConfigureAwait(false);
            return API.DeserializeJSon<WebhookDelivery>(json);
        }

        #endregion
    }
    public class WCObject : WCObject<Coupon, Customer, Order, OrderNote, OrderRefund, Product, ProductCategory,
                                    ProductAttribute, ProductAttributeTerm, ShippingClass, ProductTag, TaxRate, TaxClass, Webhook>
    {
        public WCObject(RestAPI api) : base(api)
        {
        }
    }
}
