using System.Collections.Generic;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Readarr.Http;

namespace Readarr.Api.V1.Health
{
    [V1ApiController]
    public class HealthController : ReadarrRestControllerWithSignalR<HealthResource, HealthCheck>,
                                IHandle<HealthCheckCompleteEvent>
    {
        private readonly IHealthCheckService _healthCheckService;

        public HealthController(IBroadcastSignalRMessage signalRBroadcaster, IHealthCheckService healthCheckService)
            : base(signalRBroadcaster)
        {
            _healthCheckService = healthCheckService;
            GetResourceAll = GetHealth;
        }

        private List<HealthResource> GetHealth()
        {
            return _healthCheckService.Results().ToResource();
        }

        public void Handle(HealthCheckCompleteEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
