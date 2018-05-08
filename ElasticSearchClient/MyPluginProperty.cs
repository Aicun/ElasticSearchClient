using System.Collections.Generic;
using Nest;

namespace ElasticSearchClient
{
    public class MyPluginProperty : IProperty
    {
        public PropertyName Name { get; set; }
        public string Type { get; set; }
        public IDictionary<string, object> LocalMetadata { get; set; }

        [PropertyName("language")]
        public string  Language { get; set; }

        [PropertyName("numeric")]
        public bool Numeric { get; set; }
    }
}
