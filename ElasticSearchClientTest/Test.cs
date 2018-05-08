using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Newtonsoft.Json;
using NUnit.Framework;

namespace ElasticSearchClientTest
{
    [TestFixture]
    public class Test
    {

        private ElasticClient _client;

        [SetUp]
        public void Init()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9500/"));

            _client = new ElasticClient(settings);
        }


        [Test]
        public void LinqTest()
        {
            var ports = new[] { 9500, 9600, 9700, 9800 };
            var urls = ports.Select(port => new Uri($"http://localhost:{port}")).ToArray();

            foreach (var url in urls)
            {
                Console.WriteLine(url.AbsoluteUri);
            }
        }

        [Test]
        public void TestBuildInAnalysis()
        {
            var response = _client.Analyze(
                a => a
                    .Tokenizer("standard")
                    .Filter("stop", "lowercase")
                    .Text("C# is not a difficult language, It's SUPERIOR")
            );

            Console.WriteLine(JsonConvert.SerializeObject(response.Tokens));
        }

        [Test]
        public void TestBuildInAnalyzer()
        {
            var response = _client.Analyze(
                a => a.Analyzer("standard")
            );

            Console.WriteLine(JsonConvert.SerializeObject(response.Tokens));
        }

        [Test]
        public void TestCustomAnalyzer()
        {

        }
    }
}
