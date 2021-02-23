using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.SignalR;
using Readarr.Api.V1.Books;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Calendar
{
    [V1ApiController]
    public class CalendarController : BookControllerWithSignalR
    {
        public CalendarController(IBookService bookService,
                              ISeriesBookLinkService seriesBookLinkService,
                              IAuthorStatisticsService authorStatisticsService,
                              IMapCoversToLocal coverMapper,
                              IUpgradableSpecification upgradableSpecification,
                              IBroadcastSignalRMessage signalRBroadcaster)
        : base(bookService, seriesBookLinkService, authorStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster)
        {
            GetResourceAll = GetCalendar;
        }

        private List<BookResource> GetCalendar()
        {
            var start = DateTime.Today;
            var end = DateTime.Today.AddDays(2);
            var includeUnmonitored = Request.GetBooleanQueryParameter("unmonitored");
            var includeAuthor = Request.GetBooleanQueryParameter("includeAuthor");

            //TODO: Add Book Image support to BookControllerWithSignalR
            var includeBookImages = Request.GetBooleanQueryParameter("includeBookImages");

            var queryStart = Request.Query["Start"];
            var queryEnd = Request.Query["End"];

            if (queryStart.Any())
            {
                start = DateTime.Parse(queryStart);
            }

            if (queryEnd.Any())
            {
                end = DateTime.Parse(queryEnd);
            }

            var resources = MapToResource(_bookService.BooksBetweenDates(start, end, includeUnmonitored), includeAuthor);

            return resources.OrderBy(e => e.ReleaseDate).ToList();
        }
    }
}
