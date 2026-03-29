using Robust.Shared.Serialization;

namespace Content.Shared.Score;

/// <summary>
/// Contains the loaded score list as tuples (name, win score, kills).
/// </summary>
[NetSerializable, Serializable]
public sealed class LoadedScoreListEvent(List<(string, int, int)> scores) : EntityEventArgs
{
    public readonly List<(string, int, int)> Scores = scores;
}
