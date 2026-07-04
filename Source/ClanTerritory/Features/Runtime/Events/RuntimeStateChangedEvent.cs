using ClanTerritory.Events;

namespace ClanTerritory.Features.Runtime.Events
{
    internal sealed class RuntimeStateChangedEvent : IEvent
    {
        public RuntimeState PreviousState { get; private set; }

        public RuntimeState CurrentState { get; private set; }

        public RuntimeStateChangedEvent(
            RuntimeState previousState,
            RuntimeState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }
}