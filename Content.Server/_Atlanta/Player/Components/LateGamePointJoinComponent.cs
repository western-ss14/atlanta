using Content.Server.Atlanta.GameTicking.Rules;

namespace Content.Server.Atlanta.Player;

/// <summary>
/// This is used for late game connection if it needs to wait some logic before spawn.
/// Used in <seealso cref="MistGameRuleSystem"/>
/// </summary>
[RegisterComponent]
public sealed partial class LateGamePointJoinComponent : Component
{
}
