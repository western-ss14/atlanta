using System.Threading;
using Content.Shared.Roles;
using Content.Shared.Wieldable;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Atlanta.GameTicking.Rules.Components;

/// <summary>
/// Mist Game Rule.
/// </summary>
[RegisterComponent, Access(typeof(MistGameRuleSystem))]
public sealed partial class MistGameRuleComponent : Component
{
    public readonly EntProtoId PlayerSpawner = "MistGamePlayerSpawnPoint";
    public readonly string SupplyCategoryName = "MistGame";
    public readonly string PlayerMob = "MobMistPlayer";

    /// <summary>
    /// If false, it means that nobody entry game.
    /// </summary>
    [DataField]
    public bool Started = false;
    /// <summary>
    /// Contains all players minds.
    /// </summary>
    public readonly List<(EntityUid, string)> Minds = [];

    /// <summary>
    /// Contains currently alive players
    /// </summary>
    public readonly List<EntityUid> AlivePlayers = [];

    /// <summary>
    /// Contains records about player deaths. Uses in round-end manifest.
    /// </summary>
    public readonly List<string> PlayersGrave = [];

    /// <summary>
    /// Ratio for waves difficulty after every wave.
    /// </summary>
    public readonly float Escalation = 1.08f;

    [DataField]
    public TimeSpan SupplyTiming = TimeSpan.FromMinutes(3);

    [DataField]
    public TimeSpan SmoothingSupplyTiming = TimeSpan.FromSeconds(30);

    [DataField]
    public SoundSpecifier SupplyAttention = new SoundPathSpecifier("/Audio/Atlanta/Misc/Mist/supply-helicopter.ogg");

    public CancellationTokenSource SupplyTimerToken = new();

    // Players outfit
    [DataField]
    public List<ProtoId<StartingGearPrototype>> PlayerStartingGears = [];
}
