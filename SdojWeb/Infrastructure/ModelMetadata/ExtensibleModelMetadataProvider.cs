using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SdojWeb.Infrastructure.ModelMetadata
{
    public class ExtensibleModelMetadataProvider 
        : DataAnnotationsModelMetadataProvider
    {
        private readonly IModelMetadataFilter[] _metadataFilters;

        public ExtensibleModelMetadataProvider(
            IModelMetadataFilter[] metadataFilters)
        {
            _metadataFilters = metadataFilters;
        }

        protected override System.Web.Mvc.ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            var metadata = base.CreateMetadata(attributes, containerType, modelAccessor, modelType, propertyName);

            foreach (var m in _metadataFilters)
            {
                m.TransformMetadata(metadata, attributes);
            }

            return metadata;
        }
    }
}