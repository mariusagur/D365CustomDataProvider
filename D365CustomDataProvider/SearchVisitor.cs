using Microsoft.Xrm.Sdk.Query;

namespace D365CustomDataProvider
{
    public class SearchVisitor : IQueryExpressionVisitor
    {
        public string SearchKeyWord { get; private set; }

        public QueryExpression Visit(QueryExpression query)
        {
            //Returning null will get a random result
            if (query.Criteria.Conditions.Count == 0)
                return null;

            //Get the first filter vallue
            SearchKeyWord = query.Criteria.Conditions[0].Values[0].ToString();

            return query;
        }
    }
}