using System.Collections.ObjectModel;
using Waves.Api.Models.Enums;
using Waves.Api.Models.Record;

public static class WavesRecordPlayerHelper
{
    static Dictionary<CardPoolType, (string, bool)> PoolTypeNames { get; } =
        new()
        {
            { CardPoolType.RoleActivity, ("角色活动", true) },
            { CardPoolType.WeaponsActivity, ("武器活动", false) },
            { CardPoolType.RoleResident, ("角色常驻", false) },
            { CardPoolType.WeaponsResident, ("武器常驻", false) },
            { CardPoolType.Beginner, ("新手换取", false) },
            { CardPoolType.BeginnerChoice, ("新手自选", false) },
            { CardPoolType.GratitudeOrientation, ("感恩定向", false) },
            { CardPoolType.CharacterNovice, ("角色新旅", false) },
            { CardPoolType.WeaponNovice, ("武器新旅", false) },
            { CardPoolType.CharacterCollaboration, ("角色联动", true) },
            { CardPoolType.WeaponCollaboration, ("武器联动", false) },
        };

    extension(WavesAnalysisPlayerCardItem analysisItem)
    {
        public GameRecordNavigationItem GetRecordNavItem()
        {
            return new GameRecordNavigationItem()
            {
                DisplayName = PoolTypeNames
                    .GetValueOrDefault((CardPoolType)analysisItem.PoolType, ("未知", false))
                    .Item1,
                Id = analysisItem.PoolType,
            };
        }
    }
}
