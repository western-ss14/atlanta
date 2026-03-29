using Robust.Shared.Network;

namespace Content.Server.Atlanta.GameTicking.Rules.Components;

/// <summary>
/// This is used for recording player score.
/// </summary>
[RegisterComponent, Access(typeof(ScoreRecordingRuleSystem))]
public sealed partial class ScoreRecordingRuleComponent : Component
{
    public readonly Dictionary<NetUserId, (int, int)> PlayersRecords = new(); // winScore, kills
}
