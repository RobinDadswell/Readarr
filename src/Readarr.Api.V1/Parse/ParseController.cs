using NzbDrone.Core.Parser;
using Readarr.Api.V1.Author;
using Readarr.Api.V1.Books;
using Readarr.Http;

namespace Readarr.Api.V1.Parse
{
    [V1ApiController]
    public class ParseController : ReadarrRestController<ParseResource>
    {
        private readonly IParsingService _parsingService;

        public ParseController(IParsingService parsingService)
        {
            _parsingService = parsingService;

            GetResourceSingle = Parse;
        }

        private ParseResource Parse()
        {
            var title = Request.Query["Title"].ToString();
            var parsedBookInfo = Parser.ParseBookTitle(title);

            if (parsedBookInfo == null)
            {
                return null;
            }

            var remoteBook = _parsingService.Map(parsedBookInfo);

            if (remoteBook != null)
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedBookInfo = remoteBook.ParsedBookInfo,
                    Author = remoteBook.Author.ToResource(),
                    Books = remoteBook.Books.ToResource()
                };
            }
            else
            {
                return new ParseResource
                {
                    Title = title,
                    ParsedBookInfo = parsedBookInfo
                };
            }
        }
    }
}
