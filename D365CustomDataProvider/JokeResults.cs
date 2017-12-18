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
        //https://assets.chucknorris.host/img/avatar/chuck-norris.png
        public List<string> category { get; set; }
        public string icon_url { get; set; }
        //nrxnz9iyRqqgNix3GzVajA
        public string id { get; set; }
        //http://api.chucknorris.io/jokes/nrxnz9iyRqqgNix3GzVajA
        public string url { get; set; }
        //Chuck Norris can clap with one hand.
        public string value { get; set; }
    }
}