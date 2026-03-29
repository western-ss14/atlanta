using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Server.Atlanta.Score.Events;

[ByRefEvent]
[Serializable]
public sealed class RequireScoreRecordsEvent
{
    public readonly List<(NetUserId, int, int)> ScoreRecords = [];
}
