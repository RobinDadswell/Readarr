using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Qualities;
using Readarr.Http;

namespace Readarr.Api.V1.Qualities
{
    [V1ApiController]
    public class QualityDefinitionController : ReadarrRestController<QualityDefinitionResource>
    {
        private readonly IQualityDefinitionService _qualityDefinitionService;

        public QualityDefinitionController(IQualityDefinitionService qualityDefinitionService)
        {
            _qualityDefinitionService = qualityDefinitionService;

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
        }

        private void Update(QualityDefinitionResource resource)
        {
            var model = resource.ToModel();
            _qualityDefinitionService.Update(model);
        }

        private QualityDefinitionResource GetById(int id)
        {
            return _qualityDefinitionService.GetById(id).ToResource();
        }

        private List<QualityDefinitionResource> GetAll()
        {
            return _qualityDefinitionService.All().ToResource();
        }

        [HttpPut("update")]
        public object UpdateMany([FromBody] List<QualityDefinitionResource> resource)
        {
            //Read from request
            var qualityDefinitions = resource
                .ToModel()
                .ToList();

            _qualityDefinitionService.UpdateMany(qualityDefinitions);

            return Accepted(_qualityDefinitionService.All()
                .ToResource());
        }
    }
}
