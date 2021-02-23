using System;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Readarr.Http
{
    public class VersionedApiControllerAttribute : Attribute, IRouteTemplateProvider, IEnableCorsAttribute, IApiBehaviorMetadata
    {
        public const string API_CORS_POLICY = "ApiCorsPolicy";

        public VersionedApiControllerAttribute(int version, string resource = "[controller]")
        {
            Template = $"api/v{version}/{resource}";
            PolicyName = API_CORS_POLICY;
        }

        public string Template { get; private set; }
        public int? Order => 2;
        public string Name { get; set; }
        public string PolicyName { get; set; }
    }

    public class V1ApiControllerAttribute : VersionedApiControllerAttribute
    {
        public V1ApiControllerAttribute(string resource = "[controller]")
            : base(1, resource)
        {
        }
    }
}
