using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Nest;

namespace ElasticSearchClient
{
    class Analyzer
    {
        private ElasticClient _client;

        public Analyzer(ElasticClient client)
        {
            _client = client;
        }

        public void BuildInBasicAnalyze()
        {
            var analyzeResponse = _client.Analyze(a => a.Analyzer("standard").Text("F# is THE SUPERIOR language :)"));
            PrintResponse(analyzeResponse);
        }

        public void BuildInBasicAnalysisComponents()
        {
            var analyzeResponse = _client.Analyze(
                a => a.Tokenizer("standard")
                    //.Filters("standard")
                    .Text("F# is THE SUPERIOR language :)"));
            PrintResponse(analyzeResponse);
        }

        public void CustomAnalyzer()
        {
            _client.CloseIndex("people");
            _client.UpdateIndexSettings(
                "people", i =>
                    i.IndexSettings(
                        s =>
                            s.Analysis(
                                a =>
                                    a.CharFilters(
                                        cf =>
                                            cf.Mapping(
                                                "my_char_filter", m =>
                                                    m.Mappings("F# => FSharp")
                                            )
                                    ).TokenFilters(
                                        tf =>
                                            tf.Synonym(
                                                "my_synonym", sf =>
                                                    sf.Synonyms("superior", "great")
                                            )
                                    ).Analyzers(
                                        an =>
                                            an.Custom(
                                                "my_analyzer", ca =>
                                                    ca.Tokenizer("standard")
                                                        .CharFilters("my_char_filter")
                                                        .Filters("lowercase", "stop", "my_synonym")
                                            )
                                    )
                            )
                    )
            );
            _client.OpenIndex("people");
            _client.ClusterHealth(
                h =>
                    h.WaitForStatus(WaitForStatus.Green)
                        .Index("people")
                        .Timeout(TimeSpan.FromSeconds(5))
            );

            var analyzeResponse = _client.Analyze(
                a =>
                    a.Index("people")
                        .Analyzer("my_analyzer")
                        .Text("F# is THE SUPERIOR language :)")
            );

            PrintResponse(analyzeResponse);
            Console.WriteLine("########");

            var analyzeRequest = new AnalyzeRequest("people")
            {
                Analyzer = "standard",
                Text = new []{"F# is THE SUPERIOR languaage :) HH"},
                Explain = true
            };
            analyzeResponse = _client.Analyze(analyzeRequest);
            PrintResponse(analyzeResponse);
        }

        public void IndexProgrammingLanguage()
        {
            _client.CreateIndex(
                "questions", c => c.Settings(
                    s => s
                        .Analysis(
                            a => a
                                .CharFilters(
                                    cf => cf
                                        .Mapping(
                                            "programming_language", m => m
                                                .Mappings(
                                                        new[]
                                                        {
                                                            "c# => csharp",
                                                            "C# => Csharp"
                                                        }
                                                    )
                                            )
                                    )
                                .Analyzers(an => an
                                    .Custom("index_question", cu => cu
                                        .CharFilters("programming_language", "html_strip")
                                        .Tokenizer("standard")
                                        .Filters("standard", "lowercase", "stop")
                                    )
                                    .Custom("search_question", cu => cu
                                        .CharFilters("programming_language")
                                        .Tokenizer("standard")
                                        .Filters("standard", "lowercase", "stop")
                                    )
                                )
                            )
                    )
                .Mappings(m => m
                    .Map<Question>(mm => mm
                        .AutoMap()
                        .Properties(p => p
                                .Text(
                                    t => t
                                        .Name(n => n.Body)
                                        .Analyzer("index_question")
                                        .SearchAnalyzer("search_question")
                                )
                            )
                        )
                    )
            );

        }

        private void PrintResponse(IAnalyzeResponse response)
        {
            foreach (var token in response.Tokens)
            {
                Console.WriteLine($"{token.Token}");
            }
        }
    }
}
