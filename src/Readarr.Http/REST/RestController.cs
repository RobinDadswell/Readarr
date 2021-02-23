using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore;

namespace Readarr.Http.REST
{
    public abstract class RestController<TResource> : Controller
        where TResource : RestResource, new()
    {
        // See src/Readarr.Api.V1/Queue/QueueModule.cs
        private static readonly HashSet<string> VALID_SORT_KEYS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "authors.sortname", //Workaround authors table properties not being added on isValidSortKey call
            "timeleft",
            "estimatedCompletionTime",
            "protocol",
            "indexer",
            "downloadClient",
            "quality",
            "status",
            "title",
            "progress"
        };

        private readonly HashSet<string> _excludedKeys = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
        {
            "page",
            "pageSize",
            "sortKey",
            "sortDirection",
            "filterKey",
            "filterValue",
        };

        protected Action<int> DeleteResource { get; set; }
        protected Func<int, TResource> GetResourceById { get; set; }
        protected Func<List<TResource>> GetResourceAll { get; set; }
        protected Func<PagingResource<TResource>, PagingResource<TResource>> GetResourcePaged { get; set; }
        protected Func<TResource> GetResourceSingle { get; set; }
        protected Func<TResource, int> CreateResource { get; set; }
        protected Action<TResource> UpdateResource { get; set; }

        protected ResourceValidator<TResource> PostValidator { get; private set; }
        protected ResourceValidator<TResource> PutValidator { get; private set; }
        protected ResourceValidator<TResource> SharedValidator { get; private set; }

        protected void ValidateId(int id)
        {
            if (id <= 0)
            {
                throw new BadRequestException(id + " is not a valid ID");
            }
        }

        protected RestController()
        {
            ValidateModule();

            PostValidator = new ResourceValidator<TResource>();
            PutValidator = new ResourceValidator<TResource>();
            SharedValidator = new ResourceValidator<TResource>();
        }

        private void ValidateModule()
        {
            if (GetResourceById != null)
            {
                return;
            }

            if (CreateResource != null || UpdateResource != null)
            {
                throw new InvalidOperationException("GetResourceById route must be defined before defining Create/Update routes.");
            }
        }

        [HttpDelete("{id:int}")]
        public virtual IActionResult DeleteRoute(int id)
        {
            ValidateId(id);
            DeleteResource(id);
            return Ok();
        }

        [HttpGet("{id:int}")]
        public virtual ActionResult<TResource> GetByIdRoute(int id)
        {
            ValidateId(id);
            try
            {
                var resource = GetResourceById(id);
                if (resource == null)
                {
                    return NotFound();
                }

                return Ok(resource);
            }
            catch (ModelNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpGet]
        public virtual IActionResult GetRoute()
        {
            if (GetResourceAll != null)
            {
                return Ok(GetResourceAll());
            }
            else if (GetResourceSingle != null)
            {
                return Ok(GetResourceSingle());
            }
            else if (GetResourcePaged != null)
            {
                return Ok(GetResourcePaged(ReadPagingResourceFromRequest()));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        [HttpPost]
        public virtual ActionResult<TResource> CreateRoute(TResource item)
        {
            ValidateResource(item);

            var id = CreateResource(item);
            var created = GetResourceById(id);
            return CreatedAtAction(nameof(GetByIdRoute), new { id = id }, created);
        }

        [HttpPut("{id:int}")]
        public  virtual ActionResult<TResource> UpdateRoute(int id, TResource item)
        {
            ValidateResource(item);

            UpdateResource(item);
            var updated = GetResourceById(id);
            return AcceptedAtAction(nameof(GetByIdRoute), new { id = id }, updated);
        }

        [HttpPut]
        public virtual ActionResult<TResource> UpdateRoute(TResource item)
        {
            ValidateResource(item);

            UpdateResource(item);
            var updated = GetResourceById(item.Id);
            return AcceptedAtAction(nameof(GetByIdRoute), new { id = item.Id }, updated);
        }

        protected void ValidateResource(TResource resource, bool skipValidate = false, bool skipSharedValidate = false)
        {
            var errors = new List<ValidationFailure>();

            if (!skipSharedValidate)
            {
                errors.AddRange(SharedValidator.Validate(resource).Errors);
            }

            if (Request.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase) && !skipValidate && !Request.Path.ToString().EndsWith("/test", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.AddRange(PostValidator.Validate(resource).Errors);
            }
            else if (Request.Method.Equals("PUT", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.AddRange(PutValidator.Validate(resource).Errors);
            }

            if (errors.Any())
            {
                throw new ValidationException(errors);
            }
        }

        private PagingResource<TResource> ReadPagingResourceFromRequest()
        {
            if (!int.TryParse(Request.Query["PageSize"].ToString(), out var pageSize))
            {
                pageSize = 10;
            }

            if (!int.TryParse(Request.Query["Page"].ToString(), out var page))
            {
                page = 1;
            }

            var pagingResource = new PagingResource<TResource>
            {
                PageSize = pageSize,
                Page = page,
                Filters = new List<PagingResourceFilter>()
            };

            if (Request.Query["SortKey"].Any())
            {
                var sortKey = Request.Query["SortKey"].ToString();

                if (!VALID_SORT_KEYS.Contains(sortKey) &&
                    !TableMapping.Mapper.IsValidSortKey(sortKey))
                {
                    throw new BadRequestException($"Invalid sort key {sortKey}");
                }

                pagingResource.SortKey = sortKey;

                // For backwards compatibility with v2
                if (Request.Query["SortDir"].Any())
                {
                    pagingResource.SortDirection = Request.Query["SortDir"].ToString()
                                                          .Equals("Asc", StringComparison.InvariantCultureIgnoreCase)
                                                       ? SortDirection.Ascending
                                                       : SortDirection.Descending;
                }

                // v3 uses SortDirection instead of SortDir to be consistent with every other use of it
                if (Request.Query["SortDirection"].Any())
                {
                    pagingResource.SortDirection = Request.Query["SortDirection"].ToString()
                                                          .Equals("ascending", StringComparison.InvariantCultureIgnoreCase)
                                                       ? SortDirection.Ascending
                                                       : SortDirection.Descending;
                }
            }

            // For backwards compatibility with v2
            if (Request.Query["FilterKey"].Any())
            {
                var filter = new PagingResourceFilter
                {
                    Key = Request.Query["FilterKey"].ToString()
                };

                if (Request.Query["FilterValue"].Any())
                {
                    filter.Value = Request.Query["FilterValue"].ToString();
                }

                pagingResource.Filters.Add(filter);
            }

            // v3 uses filters in key=value format
            foreach (var pair in Request.Query)
            {
                if (_excludedKeys.Contains(pair.Key))
                {
                    continue;
                }

                pagingResource.Filters.Add(new PagingResourceFilter
                {
                    Key = pair.Key,
                    Value = pair.Value.ToString()
                });
            }

            return pagingResource;
        }
    }
}
