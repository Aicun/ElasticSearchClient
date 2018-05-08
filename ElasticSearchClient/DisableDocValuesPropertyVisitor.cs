using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nest;

namespace ElasticSearchClient
{
    class DisableDocValuesPropertyVisitor: NoopPropertyVisitor
    {
        public override void Visit(INumberProperty type, PropertyInfo propertyInfo, ElasticsearchPropertyAttributeBase attribute)
        {
            type.DocValues = false;
        }

        public override void Visit(IBooleanProperty type, PropertyInfo propertyInfo, ElasticsearchPropertyAttributeBase attribute)
        {
            type.DocValues = false;
        }
    }

    /// <summary>
    /// create a visitor that maps all CLR types to an Elasticsearch text datatype 
    /// </summary>
    public class EverythingIsATextPropertyVisitor : NoopPropertyVisitor
    {
        public override IProperty Visit(PropertyInfo propertyInfo, ElasticsearchPropertyAttributeBase attribute) => new TextProperty();
    }

    public class IngnorePropertiesVisitor<T> : NoopPropertyVisitor
    {
        public override bool SkipProperty(PropertyInfo propertyInfo, ElasticsearchPropertyAttributeBase attribute)
        {
            return propertyInfo?.DeclaringType != typeof(T);
        }
    }

}
