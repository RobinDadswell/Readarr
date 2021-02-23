using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Books;
using Readarr.Http;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Series
{
    [V1ApiController]
    public class SeriesController : ReadarrRestController<SeriesResource>
    {
        protected readonly ISeriesService _seriesService;

        public SeriesController(ISeriesService seriesService)
        {
            _seriesService = seriesService;

            GetResourceAll = GetSeries;
        }

        private List<SeriesResource> GetSeries()
        {
            var authorIdQuery = Request.Query["AuthorId"];

            if (!authorIdQuery.Any())
            {
                throw new BadRequestException("authorId must be provided");
            }

            int authorId = Convert.ToInt32(authorIdQuery);

            return _seriesService.GetByAuthorId(authorId).ToResource();
        }
    }
}
