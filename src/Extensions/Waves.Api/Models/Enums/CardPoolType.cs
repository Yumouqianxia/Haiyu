namespace Waves.Api.Models.Enums;

public enum CardPoolType : int
{
    /// <summary>
    /// 角色活动
    /// </summary>
    RoleActivity = 1,

    /// <summary>
    /// 武器活动
    /// </summary>
    WeaponsActivity = 2,

    /// <summary>
    /// 角色常驻
    /// </summary>
    RoleResident = 3,

    /// <summary>
    /// 武器常驻
    /// </summary>
    WeaponsResident = 4,

    /// <summary>
    /// 新手换取
    /// </summary>
    Beginner = 5,

    /// <summary>
    /// 新手自选
    /// </summary>
    BeginnerChoice = 6,

    /// <summary>
    /// 感恩定向
    /// </summary>
    GratitudeOrientation = 7,

    /// <summary>
    /// 角色新旅
    /// </summary>
    CharacterNovice = 8,

    /// <summary>
    /// 武器新旅
    /// </summary>
    WeaponNovice = 9,

    /// <summary>
    /// 角色联动
    /// </summary>
    CharacterCollaboration = 10,

    /// <summary>
    /// 武器联动
    /// </summary>
    WeaponCollaboration = 11,
}


public static class CardPoolTypeValues
{
    public static readonly CardPoolType[] All =
    [
        CardPoolType.RoleActivity,
        CardPoolType.WeaponsActivity,
        CardPoolType.RoleResident,
        CardPoolType.WeaponsResident,
        CardPoolType.Beginner,
        CardPoolType.BeginnerChoice,
        CardPoolType.GratitudeOrientation,
        CardPoolType.CharacterNovice,
        CardPoolType.WeaponNovice,
        CardPoolType.CharacterCollaboration,
        CardPoolType.WeaponCollaboration,
    ];
}
