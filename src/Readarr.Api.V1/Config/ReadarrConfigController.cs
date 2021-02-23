using System.Linq;
using System.Reflection;
using NzbDrone.Core.Configuration;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Config
{
    public abstract class ReadarrConfigController<TResource> : RestController<TResource>
        where TResource : RestResource, new()
    {
        private readonly IConfigService _configService;

        protected ReadarrConfigController(IConfigService configService)
        {
            _configService = configService;

            GetResourceSingle = GetConfig;
            GetResourceById = GetConfig;
            UpdateResource = SaveConfig;
        }

        private TResource GetConfig()
        {
            var resource = ToResource(_configService);
            resource.Id = 1;

            return resource;
        }

        protected abstract TResource ToResource(IConfigService model);

        private TResource GetConfig(int id)
        {
            return GetConfig();
        }

        private void SaveConfig(TResource resource)
        {
            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            _configService.SaveConfigDictionary(dictionary);
        }
    }
}
