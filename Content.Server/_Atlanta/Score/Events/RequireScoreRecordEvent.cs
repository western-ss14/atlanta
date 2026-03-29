using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Server.Atlanta.Score.Events;

[ByRefEvent]
[Serializable]
public sealed class RequireScoreRecordEvent(NetUserId userId)
{
    public readonly NetUserId UserId = userId;

    public int WinScore = 0;
    public int Kills = 0;
}
