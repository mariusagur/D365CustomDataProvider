using System.Net.Http;

namespace D365CustomDataProvider
{
    public class HttpHelper
    {
        public static HttpClient GetHttpClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Connection", "close");

            return client;
        }
    }
}