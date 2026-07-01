namespace ClanTerritory.Abstractions
{
    public interface IProvider
    {
        string Name { get; }

        bool IsAvailable { get; }
    }
}