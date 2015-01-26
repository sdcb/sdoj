using SdojWeb.Infrastructure.ModelMetadata.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SdojWeb.Infrastructure.ModelMetadata.Filters
{
    public class RenderModelFilter : IModelMetadataFilter
    {
        public void TransformMetadata(System.Web.Mvc.ModelMetadata metadata, IEnumerable<Attribute> attributes)
        {
            var renderModelAttribute = attributes.OfType<RenderModeAttribute>().FirstOrDefault();
            if (renderModelAttribute != null)
            {
                var renderMode = renderModelAttribute.RenderMode;
                switch (renderMode)
                {
                    case RenderMode.DisplayModeOnly:
                        metadata.ShowForDisplay = true;
                        metadata.ShowForEdit = false;
                        break;
                    case RenderMode.EditModeOnly:
                        metadata.ShowForDisplay = false;
                        metadata.ShowForEdit = true;
                        break;
                    case RenderMode.Neither:
                        metadata.ShowForDisplay = false;
                        metadata.ShowForEdit = false;
                        break;
                }
            }
        }
    }
}