using Robust.Shared.Player;

namespace Content.Server.Atlanta.Player.Events;

public sealed class NewConnectedPlayer(ICommonSession session)
{
    public readonly ICommonSession Session = session;
}
