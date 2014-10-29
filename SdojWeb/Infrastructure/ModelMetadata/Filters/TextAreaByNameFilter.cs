using System;
using System.Collections.Generic;

namespace SdojWeb.Infrastructure.ModelMetadata.Filters
{
    // 暂时没有用。
    //public class TextAreaByNameFilter : IModelMetadataFilter
    public class TextAreaByNameFilter
    {
        private static readonly HashSet<string> TextAreaFieldNames =
            new HashSet<string>
            {
                "body", 
            };

        public void TransformMetadata(System.Web.Mvc.ModelMetadata metadata, IEnumerable<Attribute> attributes)
        {
            if (!string.IsNullOrEmpty(metadata.PropertyName) &&
                string.IsNullOrEmpty(metadata.DataTypeName) &&
                TextAreaFieldNames.Contains(metadata.PropertyName.ToLower()))
            {
                metadata.DataTypeName = "MultilineText";
            }
        }
    }
}