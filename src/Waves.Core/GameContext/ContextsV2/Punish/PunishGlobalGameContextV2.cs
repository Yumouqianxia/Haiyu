namespace Waves.Core.GameContext.ContextsV2.Punish;

public class PunishGlobalGameContextV2 : KuroGameContextBaseV2
{
    public override string GameContextNameKey => nameof(PunishGlobalGameContextV2);
    public PunishGlobalGameContextV2(KuroGameApiConfig config, string name)
        : base(config, name) { }

    public override Type ContextType => typeof(PunishGlobalGameContextV2);
    public override GameType GameType => GameType.Punish;
}