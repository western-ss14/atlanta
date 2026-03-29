using Content.Server.Atlanta.RoyalBattle.Systems;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Atlanta.GameTicking.Rules.Components;

/// <summary>
/// This is used for royal battle setup.
/// </summary>
[RegisterComponent, Access(typeof(RoyalBattleRuleSystem), typeof(RbZoneSystem))]
public sealed partial class RoyalBattleRuleComponent : Component
{
    [DataField("gameState")]
    public RoyalBattleGameState GameState = RoyalBattleGameState.InLobby;

    [DataField("lobbyMapName")]
    public string LobbyMapPath = "Maps/_Atlanta/lobby.yml";

    [DataField]
    public MapId? LobbyMapId;

    [DataField("battleMapId")]
    public MapId? MapId;

    [DataField("center")]
    public EntityUid? Center;

    [DataField("startupTime")]
    public TimeSpan StartupTime = TimeSpan.FromMinutes(1);

    #region Players

    [DataField]
    public List<(EntityUid, string)> PlayersMinds = [];

    [DataField]
    public List<EntityUid> AlivePlayers = [];

    [DataField]
    public List<EntityUid> DeadPlayers = [];

    [DataField]
    public Dictionary<EntityUid, NetUserId> PlayersNetUserIds = [];

    [DataField]
    public Dictionary<EntityUid, string> AttachedNames = [];

    #endregion

    [DataField("availableSpawners")]
    public List<EntityUid> AvailableSpawners = [];
    /// <summary>
    /// The gear all players spawn with.
    /// </summary>
    [DataField("gear", customTypeSerializer: typeof(PrototypeIdSerializer<StartingGearPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public string Gear = "RbFighterGear";

    [DataField("restartTime")]
    public TimeSpan RestartTime = TimeSpan.FromSeconds(30);

    #region Sound

    [DataField("greetingSound")]
    public SoundSpecifier GreetingSound = new SoundPathSpecifier("/Audio/_Atlanta/Misc/RoyalBattle/rb_greeting.ogg");

    [DataField("loosingSound")]
    public SoundSpecifier LoosingSound = new SoundPathSpecifier("/Audio/_Atlanta/Misc/RoyalBattle/rb_loose.ogg");

    [DataField("zoneStartSound")]
    public SoundSpecifier ZoneStartSound = new SoundPathSpecifier("/Audio/_Atlanta/Misc/RoyalBattle/rb_zone_start.ogg");

    [DataField("zoneStopSound")]
    public SoundSpecifier ZoneStopSound = new SoundPathSpecifier("/Audio/_Atlanta/Misc/RoyalBattle/rb_zone_stop.ogg");

    [DataField("winnerSound")]
    public SoundSpecifier WinnerSound = new SoundPathSpecifier("/Audio/_Atlanta/Misc/RoyalBattle/rb_winner.ogg");

    [DataField("deathSound")]
    public SoundSpecifier DeathSound = new SoundPathSpecifier("/Audio/_Atlanta/Misc/RoyalBattle/rb_death.ogg");

    // DOOM
    [DataField("musicEntry")]
    public SoundSpecifier MusicEntry = new SoundPathSpecifier("/Audio/_Atlanta/Misc/RoyalBattle/doom_opening.ogg");

    [DataField("musicLoop")]
    public SoundSpecifier MusicLoop = new SoundPathSpecifier("/Audio/_Atlanta/Misc/RoyalBattle/doom_loop.ogg");

    [DataField("musicClosing")]
    public SoundSpecifier MusicClosing = new SoundPathSpecifier("/Audio/_Atlanta/Misc/RoyalBattle/doom_end.ogg");

    public TimeSpan LoopTimer = TimeSpan.Zero;

    #endregion
}

public sealed class RoyalBattleStartEvent : EntityEventArgs
{
}

public enum RoyalBattleGameState
{
    InLobby,
    InGame,
    IsEnd
}
