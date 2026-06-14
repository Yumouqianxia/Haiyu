namespace Waves.Core.Contracts;

public interface ILocalGameRole
{
    public string ServerName { get; set; }
    public GameType Type { get; set; }
}


public interface ILocalGamerPlayer
{
    public string Id { get; set; }

    public string ServerName { get; set; }

    public GameType Type { get; set; }
    public string RoleName { get; set; }
}