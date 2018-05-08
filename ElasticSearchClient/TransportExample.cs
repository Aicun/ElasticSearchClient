using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Elasticsearch.Net;
using Nest;

namespace ElasticSearchClient
{
    public class TransportExample
    {
        private readonly Transport<ConnectionSettings> _transport;

        private readonly ElasticClient _elasticClient;

        public TransportExample()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9500/"));

            _transport = new Transport<ConnectionSettings>(settings);

            //_elasticClient = new ElasticClient(_transport);

            _elasticClient = CreateElasticSearch();
        }

        public void LowerQueryUsingTransport()
        {
            var response = _transport.Request<SearchResponse<Employee>>(HttpMethod.GET, "/people/_search");
            foreach (var bodyHit in response.Hits)
            {
                var em = bodyHit.Source;
                Console.WriteLine("{0} {1}", em.FirstName, em.LastName);
            }
        }

        public void LowerQueryUsingClient()
        {
            var response = _elasticClient.LowLevel.DoRequest<StringResponse>(HttpMethod.GET, "people/_search");

            Console.WriteLine(response.Body);
        }

        private ElasticClient CreateElasticSearch()
        {
            var uris = Enumerable.Range(9500, 1).Select(port => new Uri($"http://localhost:{port}")).ToArray();
            var connectionPool = new SniffingConnectionPool(uris);
            var settings = new ConnectionSettings(connectionPool, new InMemoryConnection());
            return new ElasticClient(settings);
        }

        private void Task()
        {
            var semaphoreSlim =new SemaphoreSlim(1, 1);

        }
    }
}
