using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Data.Exceptions;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace D365CustomDataProvider
{
    public class Retrieve : IPlugin
    {
        #region Secure/Unsecure Configuration Setup
        private string _secureConfig = null;
        private string _unsecureConfig = null;

        public Retrieve(string unsecureConfig, string secureConfig)
        {
            _secureConfig = secureConfig;
            _unsecureConfig = unsecureConfig;
        }
        #endregion
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            try
            {
                EntityReference target = (EntityReference)context.InputParameters["Target"];

                tracer.Trace($"Target: {target.Id.ToString()}");

                JokeIdHelper jokeIdHelper = new JokeIdHelper();

                var webId = jokeIdHelper.GetWebIdByGuid(target.Id.ToString());

                if (string.IsNullOrEmpty(webId))
                    throw new InvalidPluginExecutionException("Could not find joke webId");

                tracer.Trace($"JokeWebId found: {webId}");

                var getJokeByIdTask = Task.Run(async () => await GetJokesById(tracer, webId));
                Task.WaitAll(getJokeByIdTask);

                var result = getJokeByIdTask.Result;

                tracer.Trace($"Joke found: {result.id}");

                Entity joke = JokeHelper.CreateJoke(tracer, result, jokeIdHelper);

                tracer.Trace($"Joke created: {joke.Id}");

                context.OutputParameters["BusinessEntity"] = joke;
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        private static async Task<Result> GetJokesById(ITracingService tracer, string id)
        {
            using (HttpClient client = HttpHelper.GetHttpClient())
            {
                string url = $"https://api.chucknorris.io/jokes/{id}";
                tracer.Trace(url);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

                HttpResponseMessage response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    throw new GenericDataAccessException("Chuck Norris stopped this from happening");

                string html = response.Content.ReadAsStringAsync().Result;

                string value = GetFirstParagraph(html);

                Result result = new Result
                {
                    id = id,
                    value = value,
                    url = url
                };

                return result;
            }
        }

        private static string GetFirstParagraph(string file)
        {
            Match m = Regex.Match(file, @"<p>\s*(.+?)\s*</p>");
            return m.Success
                ? m.Groups[1].Value
                : String.Empty;
        }
    }
}