using ClanTerritory.Events;
using ClanTerritory.Features.Runtime.Events;
using ClanTerritory.Utils;

namespace ClanTerritory.Features.Diagnostics
{
    internal sealed class RuntimeStateLogger :
        IEventHandler<RuntimeStateChangedEvent>
    {
        public void Handle(RuntimeStateChangedEvent eventData)
        {
            if (eventData == null)
                return;

            ModLog.Info(
                "[Runtime] " +
                eventData.PreviousState +
                " -> " +
                eventData.CurrentState);
        }
    }
}