using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Server.Atlanta.Score.Events;

[ByRefEvent]
[Serializable]
public sealed class MakeScoreRecordEvent(NetUserId userId, ScoreRecordType scoreRecordType, int count)
{
    public readonly NetUserId UserId = userId;
    public readonly ScoreRecordType ScoreRecordType = scoreRecordType;
    public readonly int Count = count;
}

public enum ScoreRecordType : byte
{
    WinScore,
    Kill,
}
