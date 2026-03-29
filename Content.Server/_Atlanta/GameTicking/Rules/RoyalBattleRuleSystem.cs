using System.Linq;
using Content.Server.Atlanta.GameTicking.Rules.Components;
using Content.Server.Clothing.Systems;
using Content.Server.Atlanta.Score;
using Content.Server.Audio;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.KillTracking;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.RoundEnd;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Atlanta.RoyalBattle.Components;
using Content.Shared.Audio;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.CCVar;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Atlanta.GameTicking.Rules;

public sealed class RoyalBattleRuleSystem : GameRuleSystem<RoyalBattleRuleComponent>
{
    private static readonly int WinMaxPrice = 5;
    private static readonly ProtoId<DamageGroupPrototype> AirlossDamageGroup = "Airloss";

    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoaderSystem = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly StationJobsSystem _jobsSystem = default!;
    [Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ScoreSystem _scoreSystem = default!;
    [Dependency] private readonly OutfitSystem _outfitSystem = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RbZoneComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<RoyalBattleRuleComponent, ComponentStartup>(OnRuleStartup);

        SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnBeforeSpawn);
        SubscribeLocalEvent<KillReportedEvent>(OnKillReport);

        SubscribeLocalEvent<RoyalBattleRuleComponent, ObjectivesTextGetInfoEvent>(OnObjectivesTextGetInfo);
        SubscribeLocalEvent<RoyalBattleRuleComponent, ObjectivesTextPrependEvent>(OnObjectivesTextPrepend);

        _sawmill = Logger.GetSawmill("Royal Battle Rule");
    }

    private void OnRuleStartup(Entity<RoyalBattleRuleComponent> ent, ref ComponentStartup args)
    {
        _cfg.SetCVar(CCVars.ArrivalsShuttles, false);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RoyalBattleRuleComponent>();
        while (query.MoveNext(out _, out var rb))
        {
            if (rb.GameState == RoyalBattleGameState.InLobby)
            {
                var time = rb.StartupTime - TimeSpan.FromSeconds(frameTime);

                if (time <= TimeSpan.Zero)
                {
                    _chatManager.DispatchServerAnnouncement(Loc.GetString("rb-lobby-end"));
                    _mapManager.SetMapPaused(rb.MapId!.Value, false);

                    if (TryGetRandomStation(out var chosenStation, HasComp<StationJobsComponent>))
                    {
                        var jobs = _jobsSystem.GetJobs(chosenStation.Value).Keys;

                        foreach (var job in jobs)
                        {
                            _jobsSystem.MakeUnavailableJob(chosenStation.Value, job);
                        }
                    }

                    _sound.PlayAdminGlobal(Filter.Broadcast(), _audio.GetSound(rb.GreetingSound), AudioParams.Default);

                    _sound.StopGlobalEventMusic(GlobalEventMusicType.RoyalBattleMusic, Filter.Broadcast());
                    _sound.DispatchGlobalMusic(_audio.GetSound(rb.MusicLoop),
                        GlobalEventMusicType.RoyalBattleMusic,
                        Filter.Broadcast(),
                        true);

                    foreach (var mob in rb.AlivePlayers)
                    {
                        RemComp<GodmodeComponent>(mob);
                        RemComp<PacifiedComponent>(mob);

                        if (rb.AvailableSpawners.Count > 0)
                        {
                            var spawner = _random.Pick(rb.AvailableSpawners);
                            var spawnerPosition = _transform.GetMoverCoordinates(spawner);
                            _transform.SetCoordinates(mob, spawnerPosition);
                            _transform.AttachToGridOrMap(mob);

                            rb.AvailableSpawners.Remove(spawner);
                        }
                        else
                        {
                            _sawmill.Error(
                                "No spawners! Player will be spawned on default position, but it doesn't well!");
                        }
                    }

                    RaiseLocalEvent(new RoyalBattleStartEvent());
                    rb.GameState = RoyalBattleGameState.InGame;

                    _chatManager.DispatchServerAnnouncement(
                        Loc.GetString("rb-start-battle-player-count", ("count", rb.AlivePlayers.Count)),
                        Color.Cyan);
                }
                else
                {
                    if (time < TimeSpan.FromSeconds(10))
                    {
                        var totalSecond = (int) time.TotalSeconds;

                        if (totalSecond < (int) rb.StartupTime.TotalSeconds)
                        {
                            if (totalSecond ==
                                (int) _audio.GetAudioLength(_audio.GetSound(rb.MusicEntry)).TotalSeconds - 1)
                            {
                                _sound.DispatchGlobalMusic(_audio.GetSound(rb.MusicEntry),
                                    GlobalEventMusicType.RoyalBattleMusic,
                                    Filter.Broadcast());
                            }

                            _chatManager.DispatchServerAnnouncement(Loc.GetString("rb-lobby-wait-time-remain",
                                ("seconds", totalSecond + 1)));
                        }
                    }

                    rb.StartupTime = time;
                }
            }
            else if (rb.GameState == RoyalBattleGameState.InGame)
            {
            }
        }
    }

    private void OnMapInit(EntityUid uid, RbZoneComponent component, MapInitEvent args)
    {
        _sawmill.Debug("Start process map with zone startup.");
        var query = EntityQueryEnumerator<RoyalBattleRuleComponent>();
        while (query.MoveNext(out _, out var rb))
        {
            rb.MapId = _transform.GetMapId(uid);

            _sound.StopGlobalEventMusic(GlobalEventMusicType.RoyalBattleMusic, Filter.Broadcast());

            if (TryComp<RbMiscPresetsComponent>(uid, out var rbMiscPresetsComponent))
            {
                rb.Gear = rbMiscPresetsComponent.Gear;
            }

            // load lobby
            if (!_mapLoaderSystem.TryLoadMap(new ResPath(rb.LobbyMapPath), out var lobbyMap, out _,
                    DeserializationOptions.Default))
            {
                _sawmill.Error("Couldn't load lobby map.");
                _chatManager.DispatchServerAnnouncement(Loc.GetString("rb-lobby-cant-spawn"), Color.Red);

                var roundEnd = EntityManager.EntitySysManager.GetEntitySystem<RoundEndSystem>();
                roundEnd.EndRound(TimeSpan.FromSeconds(10));

                continue;
            }

            var lobbyMapId = lobbyMap.Value.Comp.MapId;
            rb.LobbyMapId = lobbyMapId;

            if (!_mapSystem.IsInitialized(lobbyMapId))
            {
                _mapManager.DoMapInitialize(lobbyMapId);
            }

            if (!_mapSystem.IsPaused(lobbyMapId))
            {
                _mapSystem.SetPaused(lobbyMapId, false);
            }

            _mapManager.SetMapPaused(rb.MapId.Value, true);

            _chatManager.DispatchServerAnnouncement(Loc.GetString("rb-lobby-remain",
                ("seconds", (int) rb.StartupTime.TotalSeconds)));

            _scoreSystem.InitializeScoreRecording();
        }
    }

    public static void AddSpawner(RoyalBattleRuleComponent rule, EntityUid spawner)
    {
        rule.AvailableSpawners.Add(spawner);
    }

    #region Score

    private void OnKillReport(ref KillReportedEvent ev)
    {
        RegisterKill(ev.Primary);

        var query = EntityQueryEnumerator<RoyalBattleRuleComponent>();
        while (query.MoveNext(out _, out var rb))
        {
            if (rb.GameState == RoyalBattleGameState.IsEnd)
                continue;

            var player = ev.Entity;

            if (rb.AlivePlayers.Remove(player))
            {
                _damageable.TryChangeDamage(player,
                    new DamageSpecifier(new DamageSpecifier(
                        _prototypeManager.Index(AirlossDamageGroup),
                        FixedPoint2.New(200))));
            }
            else
            {
                _sawmill.Error($"Can't remove entity {player}! It mustn't happen.");
            }

            rb.DeadPlayers.Add(player);

            if (rb.AlivePlayers.Count <= 1)
            {
                _sawmill.Debug("Royal battle ended.");

                if (rb.AlivePlayers.Count > 0)
                {
                    var winner = rb.AlivePlayers[0];
                    var winnerName = MetaData(winner).EntityName;
                    _chatManager.DispatchServerAnnouncement(
                        Loc.GetString("rb-winner", ("winner", winnerName)),
                        Color.Aqua);
                    _sound.PlayAdminGlobal(Filter.Broadcast(), _audio.GetSound(rb.WinnerSound));
                }
                else
                {
                    _chatManager.DispatchServerAnnouncement(Loc.GetString("rb-draw"), Color.Coral);
                }

                _chatManager.DispatchServerAnnouncement(Loc.GetString("rb-ending-announce"), Color.Aquamarine);

                CalculateWinScore(rb);
                _scoreSystem.UploadPlayersScoreRecords();

                _sound.StopGlobalEventMusic(GlobalEventMusicType.RoyalBattleMusic, Filter.Broadcast());
                _sound.DispatchGlobalMusic(_audio.GetSound(rb.MusicClosing),
                    GlobalEventMusicType.RoyalBattleMusic,
                    Filter.Broadcast());

                rb.GameState = RoyalBattleGameState.IsEnd;

                var roundEnd = EntityManager.EntitySysManager.GetEntitySystem<RoundEndSystem>();
                roundEnd.EndRound(rb.RestartTime);
            }
            else
            {
                _chatManager.DispatchServerAnnouncement(Loc.GetString("rb-death-announce",
                        ("count", rb.AlivePlayers.Count)),
                    Color.Red);

                _audio.PlayGlobal(rb.LoosingSound, Filter.Entities(player), true);
                _audio.PlayGlobal(rb.DeathSound, Filter.Broadcast(), true);
            }
        }
    }

    private void RegisterKill(KillSource killSource)
    {
        if (killSource is KillPlayerSource killPlayerSource)
        {
            _scoreSystem.RecordKills(killPlayerSource.PlayerId, 1);
        }
    }

    #endregion

    private void OnBeforeSpawn(PlayerBeforeSpawnEvent ev)
    {
        var query = EntityQueryEnumerator<RoyalBattleRuleComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var rb, out var rule))
        {
            if (!GameTicker.IsGameRuleActive(uid, rule))
                continue;

            var newMind = _mind.CreateMind(ev.Player.UserId, ev.Profile.Name);
            _mind.SetUserId(newMind, ev.Player.UserId);

            var tryMob = _stationSpawning.SpawnPlayerCharacterOnStation(ev.Station, "RbFighter", ev.Profile);
            DebugTools.AssertNotNull(tryMob);
            var mob = tryMob!.Value;

            _mind.TransferTo(newMind, mob);
            _outfitSystem.SetOutfit(mob, rb.Gear);
            EnsureComp<KillTrackerComponent>(mob);
            string? characterName = null;

            if (!_mind.TryGetMind(mob, out var mindId, out var mind))
            {
                _sawmill.Info("Failed getting mind for picked rb player.");
            }
            else
            {
                _roleSystem.MindAddRole(mindId, "MindRoleRoyalBattle", mind);

                _chatManager.DispatchServerMessage(ev.Player, Loc.GetString("rb-rules"));

                _sawmill.Info($"Added new player {mind.CharacterName}/{mind}");

                characterName = mind.CharacterName;
            }

            EnsureComp<GodmodeComponent>(mob);
            EnsureComp<PacifiedComponent>(mob);
            rb.AlivePlayers.Add(mob);
            rb.PlayersMinds.Add((mindId, characterName ?? "?"));
            rb.PlayersNetUserIds.Add(mob, ev.Player.UserId);
            rb.AttachedNames.Add(mob, MetaData(mob).EntityName);
            ev.Handled = true;

            break;
        }
    }

    private void OnObjectivesTextGetInfo(EntityUid uid,
        RoyalBattleRuleComponent component,
        ref ObjectivesTextGetInfoEvent args)
    {
        args.Minds = component.PlayersMinds;
        args.AgentName = Loc.GetString("rb-agent-name");
    }

    private void OnObjectivesTextPrepend(EntityUid uid,
        RoyalBattleRuleComponent component,
        ref ObjectivesTextPrependEvent args)
    {
        args.Text += "\n";
        var place = 1;

        var playerList = new List<EntityUid>();
        if (component.AlivePlayers.Count == 0)
        {
            args.Text += Loc.GetString("rb-results-everyone-dead");
        }
        else
        {
            playerList.AddRange(component.AlivePlayers);
        }

        playerList.AddRange(component.DeadPlayers);

        foreach (var player in playerList)
        {
            MakeObjectivesText(player, component, place, ref args);
            place++;
        }

        _sawmill.Debug("Royal battle objectives text was successfully made.");
    }

    private void MakeObjectivesText(EntityUid player,
        RoyalBattleRuleComponent component,
        int place,
        ref ObjectivesTextPrependEvent args)
    {
        var record = _scoreSystem.LoadPlayerScoreRecord(component.PlayersNetUserIds[player]);

        if (record.WinScore != 0)
        {
            args.Text += Loc.GetString("rb-result-prise-place",
                ("place", place),
                ("player", GetPlayerName(player, component)),
                ("kills", record.Kills),
                ("price", record.WinScore));
        }
        else
        {
            args.Text += Loc.GetString("rb-results-place",
                ("place", place),
                ("player", GetPlayerName(player, component)),
                ("kills", record.Item2));
        }

        args.Text += '\n';
    }

    private string GetPlayerName(EntityUid attachedEntity, RoyalBattleRuleComponent component)
    {
        return component.AttachedNames[attachedEntity];
    }

    private void CalculateWinScore(RoyalBattleRuleComponent comp)
    {
        _sawmill.Debug("Calculating players win score.");
        var playerPlace = 1;
        var playerList = new List<EntityUid>();

        playerList.AddRange(comp.AlivePlayers);
        playerList.AddRange(comp.DeadPlayers.AsEnumerable().Reverse());

        /*
         * One player - no score
         * Two players - only first gets score
         * Three players - only first gets score
         * Four players - first and second get score
         * Five players - first three players get score
         */
        var winScoreBound = (int) (playerList.Count * 0.5f);

        _sawmill.Debug($"For {playerList.Count} set {winScoreBound} win score bound.");

        foreach (var player in playerList)
        {
            if (playerPlace > winScoreBound)
            {
                break;
            }

            var score = Math.Min(WinMaxPrice * (playerPlace / winScoreBound), WinMaxPrice);
            var userId = comp.PlayersNetUserIds[player];
            _scoreSystem.RecordWinScore(userId, score);

            _sawmill.Debug($"{userId} player on {playerPlace} got {score}.");

            playerPlace++;
        }
    }
}
