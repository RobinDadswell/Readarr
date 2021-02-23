using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Books;
using Readarr.Http;

namespace Readarr.Api.V1.Author
{
    [V1ApiController("author/import")]
    public class AuthorImportController : ReadarrRestController<AuthorResource>
    {
        private readonly IAddAuthorService _addAuthorService;

        public AuthorImportController(IAddAuthorService addAuthorService)
        {
            _addAuthorService = addAuthorService;
        }

        [HttpPost]
        public object Import([FromBody] List<AuthorResource> resource)
        {
            var newAuthors = resource.ToModel();

            return _addAuthorService.AddAuthors(newAuthors).ToResource();
        }
    }
}
