namespace Waves.Core.GameContext;

public static class GameContextFactory
{
    public static string GameBassPath { get; set; }

    #region 新核心

    internal static PunishMainGameContextV2 GetMainPunishGameContextV2() =>
        new PunishMainGameContextV2(
            KuroGameApiConfig.MainBGRConfig,
            nameof(PunishMainGameContextV2)
        )
        {
            GamerConfigPath = GameContextFactory.GameBassPath + "\\MainPGRConfig",
            IsLimitSpeed = false,
        };

    internal static PunishBiliBiliGameContextV2 GetBiliBiliPunishGameContextV2() =>
        new PunishBiliBiliGameContextV2(
            KuroGameApiConfig.BiliBiliBGRConfig,
            nameof(PunishBiliBiliGameContextV2)
        )
        {
            GamerConfigPath = GameContextFactory.GameBassPath + "\\BilibiliPRGConfig",
            IsLimitSpeed = false,
        };

    internal static PunishGlobalGameContextV2 GetGlobalPunishGameContextV2() =>
        new PunishGlobalGameContextV2(
            KuroGameApiConfig.GlobalBGRConfig,
            nameof(PunishGlobalGameContextV2)
        )
        {
            GamerConfigPath = GameContextFactory.GameBassPath + "\\GlokbalPGRConfig",
            IsLimitSpeed = false,
        };

    internal static PunishTwGameContextV2 GetTwPunishGameContextV2() =>
        new PunishTwGameContextV2(KuroGameApiConfig.TWBGRConfig, nameof(PunishTwGameContextV2))
        {
            GamerConfigPath = GameContextFactory.GameBassPath + "\\TwPGRConfig",
            IsLimitSpeed = false,
        };

    internal static ContextsV2.Waves.WavesMainGameContextV2 GetMainWavesGameContextV2() =>
        new ContextsV2.Waves.WavesMainGameContextV2(
            KuroGameApiConfig.MainAPiConfig,
            nameof(WavesMainGameContextV2)
        )
        {
            GamerConfigPath = GameContextFactory.GameBassPath + "\\MainConfig",
            IsLimitSpeed = false,
        };

    internal static ContextsV2.Waves.WavesBiliBiliGameContextV2 GetBilibiliWavesGameContextV2() =>
        new ContextsV2.Waves.WavesBiliBiliGameContextV2(
            KuroGameApiConfig.BilibiliConfig,
            nameof(WavesBiliBiliGameContextV2)
        )
        {
            GamerConfigPath = GameContextFactory.GameBassPath + "\\BiliBiliConfig",
            IsLimitSpeed = false,
        };

    internal static ContextsV2.Waves.WavesGlobalGameContextV2 GetWavesGlobalGameContextV2() =>
        new ContextsV2.Waves.WavesGlobalGameContextV2(
            KuroGameApiConfig.GlobalConfig,
            nameof(WavesGlobalGameContextV2)
        )
        {
            GamerConfigPath = GameContextFactory.GameBassPath + "\\GlobalConfig",
            IsLimitSpeed = false,
        };
    #endregion

    public static IReadOnlyCollection<string> GetAllLocalContextName() =>
        [
            nameof(WavesBiliBiliGameContextV2),
            nameof(WavesGlobalGameContextV2),
            nameof(WavesMainGameContextV2),
            nameof(PunishBiliBiliGameContextV2),
            nameof(PunishGlobalGameContextV2),
            nameof(PunishMainGameContextV2),
            nameof(PunishTwGameContextV2),
        ];

    public static IReadOnlyCollection<string> GetAllCloudContextName() => [
        nameof(KuroCloudGameContext)
        ];
}
