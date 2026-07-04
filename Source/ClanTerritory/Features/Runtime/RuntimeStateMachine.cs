using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Events;

namespace ClanTerritory.Features.Runtime
{
    internal sealed class RuntimeStateMachine
    {
        private readonly EventBus _eventBus;
        private RuntimeState _state;

        public RuntimeStateMachine(EventBus eventBus)
        {
            _eventBus = eventBus;
            _state = RuntimeState.PluginLoaded;
        }

        public RuntimeState State
        {
            get { return _state; }
        }

        public void SetState(RuntimeState state)
        {
            if (_state == state)
                return;

            RuntimeState previousState = _state;
            _state = state;

            _eventBus.Publish(
                new RuntimeStateChangedEvent(previousState, state));
        }
    }
}