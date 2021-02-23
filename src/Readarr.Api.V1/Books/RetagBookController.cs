using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles;
using Readarr.Http;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Books
{
    [V1ApiController("retag")]
    public class RetagBookController : ReadarrRestController<RetagBookResource>
    {
        private readonly IAudioTagService _audioTagService;

        public RetagBookController(IAudioTagService audioTagService)
        {
            _audioTagService = audioTagService;

            GetResourceAll = GetBooks;
        }

        private List<RetagBookResource> GetBooks()
        {
            if (Request.Query["bookId"].Any())
            {
                var bookId = int.Parse(Request.Query["bookId"]);
                return _audioTagService.GetRetagPreviewsByBook(bookId).Where(x => x.Changes.Any()).ToResource();
            }
            else if (Request.Query["AuthorId"].Any())
            {
                var authorId = int.Parse(Request.Query["AuthorId"]);
                return _audioTagService.GetRetagPreviewsByAuthor(authorId).Where(x => x.Changes.Any()).ToResource();
            }
            else
            {
                throw new BadRequestException("One of authorId or bookId must be specified");
            }
        }
    }
}
