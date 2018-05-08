using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nest;
using Newtonsoft.Json;

namespace ElasticSearchClient
{
    [ElasticsearchType(Name = "student")]
    class Student
    {
        [Keyword()]
        public string FirstName { get; set; }

        [Text(Name = "last_name")]
        public string LastName { get; set; }

        [Text(Ignore = true)]
        public int Age { get; set; }

        [JsonIgnore]
        public string Address { get; set; }

        public string AnotherToIgnore { get; set; }

        [Object(Store = false, Name = "c")]
        public List<Course> Courses { get; set; }
    }

    class Course
    {
        public string Name { get; set; }
        public string Code { get; set; }

    }
}
