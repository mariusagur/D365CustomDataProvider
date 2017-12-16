using Microsoft.Xrm.Sdk;
using System;

namespace D365CustomDataProvider
{
    public class JokeHelper
    {
        public static Entity CreateJoke(ITracingService tracer, Result result, JokeIdHelper jokeIdHelper)
        {
            tracer.Trace($"Result Id: {result.id}");
            var crmId = jokeIdHelper.GetGuidByWebId(result.id);

            if (crmId == Guid.Empty)
                return null;

            tracer.Trace($"JokeWebId found: {crmId}");

            return new Entity("lat_chucknorrisjoke")
            {
                ["lat_chucknorrisjokeid"] = crmId,
                ["lat_webid"] = result.id,
                ["lat_joke"] = result.value,
                ["lat_url"] = result.url,
                ["lat_name"] = result.value.Length > 25
                    ? result.value.Substring(0, 25)
                    : result.value
            };
        }
    }
}