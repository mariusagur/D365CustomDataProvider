using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Data.Exceptions;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace D365CustomDataProvider
{
    public class RetrieveMultiple : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);

            //https://api.chucknorris.io/#!
            try
            {
                QueryExpression query = context.InputParameterOrDefault<QueryExpression>("Query");

                var visitor = new SearchVisitor();
                query.Accept(visitor);

                EntityCollection results = new EntityCollection();

                JokeIdHelper jokeIdHelper = new JokeIdHelper();

                if (string.IsNullOrEmpty(visitor.SearchKeyWord))
                {
                    tracer.Trace("Getting random joke");

                    var getRandomJokeTask = Task.Run(async () => await GetRandomJoke(tracer));
                    Task.WaitAll(getRandomJokeTask);

                    Result randomJoke = getRandomJokeTask.Result;

                    Entity joke = JokeHelper.CreateJoke(tracer, randomJoke, jokeIdHelper);
                    if (joke != null)
                    {
                        tracer.Trace($"Joke created: {joke.Id}");
                        results.Entities.Add(joke);
                    }
                }
                else
                {
                    tracer.Trace($"Searching jokes for: {visitor.SearchKeyWord}");

                    var getJokesByValueTask = Task.Run(async () => await GetJokesByValue(tracer, visitor.SearchKeyWord));
                    Task.WaitAll(getJokesByValueTask);

                    Jokes jokes = getJokesByValueTask.Result;

                    tracer.Trace($"Found {jokes.total} jokes");

                    foreach (Result result in jokes.result)
                    {
                        Entity joke = JokeHelper.CreateJoke(tracer, result, jokeIdHelper);
                        if (joke == null)
                            continue;

                        tracer.Trace($"Joke created: {joke.GetAttributeValue<Guid>("lat_chucknorrisjokeid")}");

                        results.Entities.Add(joke);
                    }
                }

                context.OutputParameters["BusinessEntityCollection"] = results;
            }
            catch (Exception e)
            {
                tracer.Trace($"{e.Message} {e.StackTrace}");
                if (e.InnerException != null)
                    tracer.Trace($"{e.InnerException.Message} {e.InnerException.StackTrace}");

                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        private static async Task<Jokes> GetJokesByValue(ITracingService tracer, string keyword)
        {
            using (HttpClient client = HttpHelper.GetHttpClient())
            {
                string url = $"https://api.chucknorris.io/jokes/search?query={keyword}";
                tracer.Trace(url);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

                HttpResponseMessage response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    throw new GenericDataAccessException("Chuck Norris stopped this from happening");

                string json = response.Content.ReadAsStringAsync().Result;

                Jokes results = Utility.DeserializeObject<Jokes>(json);

                return results;
            }
        }

        private static async Task<Result> GetRandomJoke(ITracingService tracer)
        {
            using (HttpClient client = HttpHelper.GetHttpClient())
            {
                string url = "https://api.chucknorris.io/jokes/random";
                tracer.Trace(url);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));

                HttpResponseMessage response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    throw new GenericDataAccessException("Chuck Norris stopped this from happening");

                string json = response.Content.ReadAsStringAsync().Result;

                Result result = Utility.DeserializeObject<Result>(json);

                return result;
            }
        }
    }
}