using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Events;

namespace ClanTerritory.Features.Runtime.Pipeline
{
    internal sealed class RuntimePipelineCoordinator :
        IEventHandler<RuntimeStateChangedEvent>
    {
        private readonly RuntimePipeline _pipeline;

        public RuntimePipelineCoordinator(RuntimePipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public void Handle(RuntimeStateChangedEvent eventData)
        {
            if (eventData == null)
                return;

            _pipeline.Execute(eventData.CurrentState);
        }
    }
}