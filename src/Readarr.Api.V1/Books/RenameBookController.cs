using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles;
using Readarr.Http;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Books
{
    [V1ApiController("rename")]
    public class RenameBookController : ReadarrRestController<RenameBookResource>
    {
        private readonly IRenameBookFileService _renameBookFileService;

        public RenameBookController(IRenameBookFileService renameBookFileService)
        {
            _renameBookFileService = renameBookFileService;

            GetResourceAll = GetBookFiles;
        }

        private List<RenameBookResource> GetBookFiles()
        {
            int authorId;

            if (Request.Query["AuthorId"].Any())
            {
                authorId = int.Parse(Request.Query["AuthorId"]);
            }
            else
            {
                throw new BadRequestException("authorId is missing");
            }

            if (Request.Query["bookId"].Any())
            {
                var bookId = int.Parse(Request.Query["bookId"]);
                return _renameBookFileService.GetRenamePreviews(authorId, bookId).ToResource();
            }

            return _renameBookFileService.GetRenamePreviews(authorId).ToResource();
        }
    }
}
