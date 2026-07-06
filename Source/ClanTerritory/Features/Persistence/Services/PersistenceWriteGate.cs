namespace ClanTerritory.Features.Persistence.Services
{
    internal sealed class PersistenceWriteGate
    {
        public bool CanWrite { get; private set; }

        public void Open()
        {
            CanWrite = true;
        }

        public void Close()
        {
            CanWrite = false;
        }
    }
}