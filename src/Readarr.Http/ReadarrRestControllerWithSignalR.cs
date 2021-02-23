using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Readarr.Http.REST;

namespace Readarr.Http
{
    public abstract class ReadarrRestControllerWithSignalR<TResource, TModel> : ReadarrRestController<TResource>, IHandle<ModelEvent<TModel>>
        where TResource : RestResource, new()
        where TModel : ModelBase, new()
    {
        protected string Resource { get; private set; }
        private readonly IBroadcastSignalRMessage _signalRBroadcaster;

        protected ReadarrRestControllerWithSignalR(IBroadcastSignalRMessage signalRBroadcaster)
        {
            _signalRBroadcaster = signalRBroadcaster;
            Resource = new TResource().ResourceName.Trim('/');
        }

        public void Handle(ModelEvent<TModel> message)
        {
            if (!_signalRBroadcaster.IsConnected)
            {
                return;
            }

            if (message.Action == ModelAction.Deleted || message.Action == ModelAction.Sync)
            {
                BroadcastResourceChange(message.Action);
            }

            BroadcastResourceChange(message.Action, message.Model.Id);
        }

        protected void BroadcastResourceChange(ModelAction action, int id)
        {
            if (!_signalRBroadcaster.IsConnected)
            {
                return;
            }

            if (action == ModelAction.Deleted)
            {
                BroadcastResourceChange(action, new TResource { Id = id });
            }
            else
            {
                var resource = GetResourceById(id);
                BroadcastResourceChange(action, resource);
            }
        }

        protected void BroadcastResourceChange(ModelAction action, TResource resource)
        {
            if (!_signalRBroadcaster.IsConnected)
            {
                return;
            }

            if (GetType().Namespace.Contains("V1"))
            {
                var signalRMessage = new SignalRMessage
                {
                    Name = Resource,
                    Body = new ResourceChangeMessage<TResource>(resource, action),
                    Action = action
                };

                _signalRBroadcaster.BroadcastMessage(signalRMessage);
            }
        }

        protected void BroadcastResourceChange(ModelAction action)
        {
            if (!_signalRBroadcaster.IsConnected)
            {
                return;
            }

            if (GetType().Namespace.Contains("V1"))
            {
                var signalRMessage = new SignalRMessage
                {
                    Name = Resource,
                    Body = new ResourceChangeMessage<TResource>(action),
                    Action = action
                };

                _signalRBroadcaster.BroadcastMessage(signalRMessage);
            }
        }
    }
}
