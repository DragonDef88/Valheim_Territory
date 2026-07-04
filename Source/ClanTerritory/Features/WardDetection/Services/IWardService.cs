using ClanTerritory.Domain.Identifiers;
using ClanTerritory.Features.WardDetection.Models;

namespace ClanTerritory.Features.WardDetection.Services
{
    internal interface IWardService
    {
        void RegisterWard(WardModel ward);

        void UnregisterWard(WardId wardId);
    }
}