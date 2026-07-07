using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.Runtime.Registry;

namespace ClanTerritory.Features.WardMenu.Services
{
    internal interface IWardMenuService
    {
        bool IsOpen { get; }

        WardId CurrentWardId { get; }

        void Open(
            WardId wardId,
            RuntimeWard runtimeWard,
            PrivateArea privateArea,
            Player player);

        void Close();
    }
}