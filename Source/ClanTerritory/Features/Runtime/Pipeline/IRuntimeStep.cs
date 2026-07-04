using ClanTerritory.Features.Runtime;

namespace ClanTerritory.Features.Runtime.Pipeline
{
    internal interface IRuntimeStep
    {
        RuntimeState InputState { get; }

        RuntimeState OutputState { get; }

        void Execute();
    }
}