using Robust.Shared.Serialization;

namespace Content.Shared.Score;

/// <summary>
/// Used to request the score list for display in the lobby.
/// </summary>
[NetSerializable, Serializable]
public sealed class RequestScoreListEvent : EntityEventArgs
{
}
