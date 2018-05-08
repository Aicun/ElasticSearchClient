using System;
using Elasticsearch.Net;

namespace ElasticSearchClient
{
    public class LowLevelExample
    {
        private ElasticLowLevelClient _elasticLowLevelClient;
        public LowLevelExample()
        {
            var configuration = new ConnectionConfiguration(new Uri("http://localhost:9500")).RequestTimeout(TimeSpan.FromMinutes(2));
            _elasticLowLevelClient = new ElasticLowLevelClient(configuration);
        }

        public void Save(Person p)
        {
            var indexResponse = _elasticLowLevelClient.Index<StringResponse>("people", "person", PostData.Serializable(p));
            Console.WriteLine(indexResponse.Body);
        }
    }
}
