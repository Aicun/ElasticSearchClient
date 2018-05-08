using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;
using Nest.JsonNetSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ElasticSearchClient
{
    internal class Program
    {
        private ElasticClient _elasticClient;

        public ElasticClient ElasticClient => _elasticClient;

        public static void Main(string[] args)
        {

            var transport = new TransportExample();
            transport.LowerQueryUsingClient();

            /*var lowLevel = new LowLevelExample();
            lowLevel.Save(new Person{
                Id = 121,
                FirstName = "abc",
                LastName = "def"
            });*/

            /*Program p = new Program();

            p.InitElasticSearchDefault();

            p.SetTransientLogLevel(TraceLevel.Verbose);

            Search s = new Search(p.ElasticClient);
            s.CompoundSearch();*/

            //Analyzer a = new Analyzer(p.ElasticClient);
            //a.BuildInBasicAnalysisComponents();
            //a.CustomAnalyzer();

            /*p.ReadConfiguredResponse();
            p.SavePerson(p.InitPersonList());
            p.ReadPerson();
            p.ReadPerson2();
            p.ReadPersonViaLowLevel();
            p.AggregationData();*/


            /*p.SaveEntity(p.InitEmployeeList());

            Company c = new Company { Name = "SOTI" };
            p.SaveCompany(c);

            p.AutoMapping();*/

            Console.ReadKey();
        }

        public List<Person> InitPersonList()
        {
            var personList = new List<Person>
            {
                new Person
                {
                    Id = 1,
                    FirstName = "Martijn",
                    LastName = "Laarman"
                },
                new Person
                {
                    Id = 2,
                    FirstName = "Martijn",
                    LastName = "AAAAAA"
                },
                new Person
                {
                    Id = 3,
                    FirstName = "Martijn",
                    LastName = "AAAAAA"
                },
                new Person
                {
                    Id = 4,
                    FirstName = "Martijn",
                    LastName = "CCCCC"
                },
                new Person
                {
                    Id = 5,
                    FirstName = "Martijn",
                    LastName = "CCCCC"
                }
            };
            return personList;
        }


        public void InitElasticSearchDefault()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9500/"))
                .DefaultIndex("people");

            _elasticClient = new ElasticClient(settings);
        }

        public void InitElasticSearchUsingPool()
        {
            //var uri = new Uri("http://localhost:9500");
            //var pool = new SingleNodeConnectionPool(uri);

            var uris = Enumerable.Range(9500, 5).Select(port => new Uri(@"http://localhost:{port}"));
            var pool = new StaticConnectionPool(uris);
            var settings = new ConnectionSettings(pool);
            _elasticClient = new ElasticClient(settings);
        }

        public void InitElasticSearchUsingInMemory()
        {
            var uri = new Uri("http://localhost:9500");

            //using InmemoryConnection,for test purpose, return the configured response
            var response = new
            {
                took = 1,
                timed_out = false,
                _shards = new
                {
                    total = 2,
                    successful = 2,
                    failed = 0
                },
                hits = new
                {
                    total = 25,
                    max_score = 1.0,
                    hits = Enumerable.Range(1, 25).Select(
                        i => (object)new
                        {
                            _index = "people",
                            _type = "person",
                            _id = $"Project {i}",
                            _score = 1.0,
                            _source = new
                            {
                                Id = i,
                                FirstName = "Hello",
                                LastName = $"World {i}"
                            }
                        }).ToArray()
                }
            };

            var responseBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(response));
            var connection = new InMemoryConnection(responseBytes, 200);
            var connectionPool = new SingleNodeConnectionPool(uri);
            //var connSettings = new ConnectionSettings(connectionPool, connection, (builtin, settings) => new CustomSerializer()).DefaultIndex("people");
            var connSettings = new ConnectionSettings(connectionPool,
                connection,
                (builtin, settings) => new JsonNetSerializer(builtin, settings,
                    ()=> new JsonSerializerSettings(),
                    resolver => resolver.NamingStrategy = new SnakeCaseNamingStrategy()));
            _elasticClient = new ElasticClient(connSettings);
        }

        public void InitElasticSearchUsingHttp()
        {
            var uri = new Uri("http://localhost:9500");
            var connection = new MyCustomHttpConnection();
            var connectionPool = new SingleNodeConnectionPool(uri);
            var settings = new ConnectionSettings(connectionPool, connection);
            _elasticClient = new ElasticClient(settings);
        }

        public void SavePerson(List<Person> personliList)
        {
            foreach (var person in personliList)
            {
                _elasticClient.IndexDocument(person);
            }
        }

        public void ReadPerson()
        {
            var searchResponse = _elasticClient.Search<Person>(
                s => s
                    .Query(
                        q => q
                            .Match(
                                m => m
                                    .Field(f => f.LastName)
                                    .Query("AAAAAA")
                            )
                    )
            );
            PrintPerson(searchResponse);
        }

        public void ReadPerson2()
        {
            var searchRequest = new SearchRequest<Person>(Indices.All, Types.All)
            {
                From = 0,
                Size = 10,
                Query = new MatchQuery
                {
                    Field = Infer.Field<Person>(p => p.FirstName),
                    Query = "Martijn"
                }
            };

            var searchResponse = _elasticClient.Search<Person>(searchRequest);
            PrintPerson(searchResponse);
        }

        public void ReadPersonViaLowLevel()
        {
            var searchResponse = _elasticClient.LowLevel.Search<SearchResponse<Person>>(
                "people", "person", PostData.Serializable(new
                {
                    from = 0,
                    size = 10,
                    query = new
                    {
                        match = new
                        {
                            firstName = "Martijn"
                        }
                    }
                }));
            PrintPerson(searchResponse);
        }

        public void AggregationData()
        {
            Task<ISearchResponse<Person>> t = PersonAggregationAsync();
            ISearchResponse<Person> response = t.Result;
            var termsAggregation = response.Aggs.Terms("last_names");

            var buckets = termsAggregation.Buckets;

            Console.WriteLine("Aggregations counts {0}", buckets.Count);

            foreach (var bucket in buckets)
            {
                Console.WriteLine("keys {0}", bucket.Key);
            }
        }

        public void ReadConfiguredResponse()
        {
            var response = _elasticClient.Search<Person>(s => s.MatchAll());
            PrintPerson(response);
        }


        public List<Employee> InitEmployeeList()
        {
            var employeeList = new List<Employee>
            {
                new Employee()
                {
                    FirstName = "Hello",
                    LastName = "World",
                    Birthday = new DateTime(),
                    Employees = null,
                    Hours = new TimeSpan(0),
                    IsManager = true,
                    Salary = 1000
                },
                new Employee()
                {
                    FirstName = "Have",
                    LastName = "Fun",
                    Birthday = new DateTime(),
                    Employees = null,
                    Hours = new TimeSpan(0),
                    IsManager = true,
                    Salary = 2000
                }
            };
            return employeeList;
        }

        public void SaveEntity(List<Employee> entities)
        {
            foreach (var e in entities)
            {
                _elasticClient.IndexDocument(e);
            }
        }

        public void SaveCompany(Company company)
        {
            _elasticClient.IndexDocument(company);
        }

        public void AutoMapping()
        {
            var descriptor = new CreateIndexDescriptor("people").Mappings(
                mappings =>
                    mappings.Map<Company>(t => t.AutoMap())
                        .Map<Employee>(t => t.AutoMap()));
            var response = _elasticClient.IndexDocument(descriptor);
            Console.WriteLine(response);
        }

        public void AttributeMapping()
        {
            var descriptor = new CreateIndexDescriptor("people").Mappings(
                mappings =>
                    mappings.Map<Student>(s => s.AutoMap())
                        .Map<Course>(c => c.AutoMap()));
            var response = _elasticClient.IndexDocument(descriptor);
        }

        public void FluentMapping()
        {
            var descriptor = new CreateIndexDescriptor("people").Mappings(
                ms => ms.Map<Company>(
                    c => c.Properties(
                        p => p.Text(n => n.Name(co => co.Name))
                            .Object<Employee>(
                                o => o.Name(e => e.Employees)
                                    .Properties(
                                        eps => eps
                                            .Text(
                                                s => s
                                                    .Name(e => e.FirstName)
                                            )
                                            .Text(
                                                s => s
                                                    .Name(e => e.LastName)
                                            )
                                            .Number(
                                                n => n
                                                    .Name(e => e.Salary)
                                                    .Type(NumberType.Integer)
                                            )
                                    )
                            )
                    )
                )
            );
        }

        public void Visit()
        {
            var descriptor = new CreateIndexDescriptor("people").
                Mappings(p => p.Map<Employee>(e => e.AutoMap(new DisableDocValuesPropertyVisitor())));
        }

        public void PropertyToIgnore()
        {
            var descriptor = new CreateIndexDescriptor("people").Mappings(ms => ms.Map<Student>(s => s.AutoMap()));

            var connectionSettings = new ConnectionSettings(new InMemoryConnection())
                .DisableDirectStreaming()
                .DefaultMappingFor<Student>(s => s
                    .Ignore(p => p.AnotherToIgnore)
                    .PropertyName(p => p.Address,"add"));
        }


        public void SetTransientLogLevel(TraceLevel traceLevel)
        {
            var response = _elasticClient.ClusterPutSettings(d => d.Transient(t => t.Add("org.elasticsearch.action", "Debug")));
            Console.WriteLine("{0} {1}", response.ApiCall.Uri, response.ToString());
        }

        private async Task<ISearchResponse<Person>> PersonAggregationAsync()
        {
            var searchResponse = await _elasticClient.SearchAsync<Person>(
                s => s
                    .Aggregations(a => a.Terms("last_names", ta => ta.Field(f => f.LastName))));

            return searchResponse;
        }

        private void PrintPerson(ISearchResponse<Person> response)
        {
            foreach (var person in response.Documents)
            {
                Console.WriteLine("firsname {0}, lastname {1}", person.FirstName, person.LastName);
            }
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class MyCustomHttpConnection : HttpConnection
    {
        protected override void AlterServicePoint(ServicePoint requestServicePoint, RequestData requestData)
        {
            base.AlterServicePoint(requestServicePoint, requestData);
            requestServicePoint.ConnectionLimit = 1000;
            requestServicePoint.UseNagleAlgorithm = true;
        }

        protected override HttpWebRequest CreateHttpWebRequest(RequestData requestData)
        {
            var request = base.CreateWebRequest(requestData);
            request.ClientCertificates.Add(new X509Certificate());
            return request;
        }
    }
}
