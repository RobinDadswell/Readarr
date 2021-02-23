using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Datastore;
using Readarr.Http;

namespace Readarr.Api.V1.Blacklist
{
    [V1ApiController]
    public class BlacklistController : ReadarrRestController<BlacklistResource>
    {
        private readonly IBlacklistService _blacklistService;

        public BlacklistController(IBlacklistService blacklistService)
        {
            _blacklistService = blacklistService;
            GetResourcePaged = GetBlacklist;
            DeleteResource = DeleteBlacklist;
        }

        private PagingResource<BlacklistResource> GetBlacklist(PagingResource<BlacklistResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<BlacklistResource, NzbDrone.Core.Blacklisting.Blacklist>("date", SortDirection.Descending);

            return ApplyToPage(_blacklistService.Paged, pagingSpec, BlacklistResourceMapper.MapToResource);
        }

        private void DeleteBlacklist(int id)
        {
            _blacklistService.Delete(id);
        }

        [HttpDelete("bulk")]
        public object Remove([FromBody] BlacklistBulkResource resource)
        {
            _blacklistService.Delete(resource.Ids);

            return new object();
        }
    }
}
