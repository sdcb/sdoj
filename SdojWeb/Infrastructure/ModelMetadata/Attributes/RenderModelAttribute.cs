using System;

namespace SdojWeb.Infrastructure.ModelMetadata.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RenderModeAttribute : Attribute
    {
        public RenderMode RenderMode { get; set; }

        public RenderModeAttribute(RenderMode renderMode)
        {
            RenderMode = renderMode;
        }
    }

    public enum RenderMode
    {
        Any,
        EditModeOnly,
        DisplayModeOnly, 
        Neither
    }
}