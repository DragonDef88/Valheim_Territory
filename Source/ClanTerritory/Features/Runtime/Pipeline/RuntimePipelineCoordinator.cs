using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Events;

namespace ClanTerritory.Features.Runtime.Pipeline
{
    internal sealed class RuntimePipelineCoordinator :
        IEventHandler<RuntimeStateChangedEvent>
    {
        private readonly RuntimeStateMachine _stateMachine;
        private readonly RuntimePipeline _pipeline;

        public RuntimePipelineCoordinator(
            RuntimeStateMachine stateMachine,
            RuntimePipeline pipeline)
        {
            _stateMachine = stateMachine;
            _pipeline = pipeline;
        }

        public void Handle(RuntimeStateChangedEvent eventData)
        {
            if (eventData == null)
                return;

            RuntimeState nextState =
                _pipeline.Execute(eventData.CurrentState);

            if (nextState == eventData.CurrentState)
                return;

            _stateMachine.SetState(nextState);
        }
    }
}