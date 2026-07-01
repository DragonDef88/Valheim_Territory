namespace ClanTerritory.Events
{
    internal interface IEventHandler<TEvent> where TEvent : IEvent
    {
        void Handle(TEvent eventData);
    }
}