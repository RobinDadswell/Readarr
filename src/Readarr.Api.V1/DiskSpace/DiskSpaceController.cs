using System.Collections.Generic;
using NzbDrone.Core.DiskSpace;
using Readarr.Http;

namespace Readarr.Api.V1.DiskSpace
{
    [V1ApiController("diskspace")]
    public class DiskSpaceController : ReadarrRestController<DiskSpaceResource>
    {
        private readonly IDiskSpaceService _diskSpaceService;

        public DiskSpaceController(IDiskSpaceService diskSpaceService)
        {
            _diskSpaceService = diskSpaceService;
            GetResourceAll = GetFreeSpace;
        }

        private List<DiskSpaceResource> GetFreeSpace()
        {
            return _diskSpaceService.GetFreeSpace().ConvertAll(DiskSpaceResourceMapper.MapToResource);
        }
    }
}
