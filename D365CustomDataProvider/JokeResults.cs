using System.Collections.Generic;

namespace D365CustomDataProvider
{
    public class Jokes
    {
        public int total { get; set; }
        public List<Result> result { get; set; }
    }

    public class Result
    {
        public List<string> category { get; set; }
        public string icon_url { get; set; }
        public string id { get; set; }
        public string url { get; set; }
        public string value { get; set; }
    }
}