using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared.Atlanta.Player.Events;

/// <summary>
/// Ensure on player mind and insert it to late join queue.
/// </summary>
public sealed partial class JoinLateGameQueueEvent : InstantActionEvent;

/// <summary>
/// Remove mind from queue.
/// </summary>
public sealed partial class LeaveLateGameQueueEvent : InstantActionEvent;
