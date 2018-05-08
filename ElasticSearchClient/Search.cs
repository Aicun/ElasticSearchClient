using System;
using System.Linq;
using Nest;

namespace ElasticSearchClient
{
    class Search
    {
        private ElasticClient _client;

        public Search(ElasticClient client)
        {
            _client = client;
        }

        public void SearchRange()
        {
            var searchResponse = _client.Search<Employee>(
                s =>
                    s.Query(
                        q =>
                            q.Range(
                                r =>
                                    r.Field(f => f.Salary)
                                        .GreaterThanOrEquals(1000)
                            )
                    )
            );
            PrintResponse(searchResponse);
        }

        public void CompoundSearch()
        {
            var searchResponse = _client.Search<Employee>(
                s =>
                    s.Query(
                        q =>
                            q.Bool(
                                b =>
                                    b.Must(
                                            mu =>
                                                mu.Match(
                                                    m =>
                                                        m.Field(e => e.FirstName)
                                                            .Query("Have")
                                                ),
                                            mu =>
                                                mu.Term(t => t.Field(e => e.IsManager).Value(true))
                                        )
                                        .Filter(
                                            f =>
                                                f.Range(
                                                    r =>
                                                        r.Field(e => e.Salary)
                                                            .GreaterThanOrEquals(2000)
                                                )
                                        )
                            )
                    )
            );
            //same with the previous one
            var searchResponse2 = _client.Search<Employee>(
                s =>
                    s.Query(
                        q =>
                            q.Match(
                                m =>
                                    m.Field(e => e.FirstName)
                                     .Query("Have")
                            ) && q.Term(
                                t => t.IsManager,true
                            ) && q.Range(
                                r =>
                                    r.Field(e => e.Salary)
                                        .GreaterThanOrEquals(2000)
                            )
                    )
            );

            var searchResponse3 = _client.Search<Employee>(
                new SearchRequest<Employee>
                {
                    From = 0,
                    Size = 10,
                    Query = new TermQuery { Field = "firstName", Value = "hello"} ||
                            new TermQuery { Field = "lastName", Value = "fun"}
                });
            PrintResponse(searchResponse3);
        }

        private void PrintResponse<T>(ISearchResponse<T> response) where T : class
        {
            foreach (var document in response.Documents)
            {
                Employee e = document as Employee;
                Console.WriteLine($"{e.FirstName}, {e.LastName}");
            }

            var source = response.Hits.Select(h => h.Source);
            foreach (var s in source)
            {
                var e = s as Employee;
                Console.WriteLine(e.FirstName);
            }
        }
    }
}
