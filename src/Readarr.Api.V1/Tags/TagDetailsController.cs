using System.Collections.Generic;
using NzbDrone.Core.Tags;
using Readarr.Http;

namespace Readarr.Api.V1.Tags
{
    [V1ApiController("tag/detail")]
    public class TagDetailsController : ReadarrRestController<TagDetailsResource>
    {
        private readonly ITagService _tagService;

        public TagDetailsController(ITagService tagService)
        {
            _tagService = tagService;

            GetResourceById = GetTagDetails;
            GetResourceAll = GetAll;
        }

        private TagDetailsResource GetTagDetails(int id)
        {
            return _tagService.Details(id).ToResource();
        }

        private List<TagDetailsResource> GetAll()
        {
            var tags = _tagService.Details().ToResource();

            return tags;
        }
    }
}
