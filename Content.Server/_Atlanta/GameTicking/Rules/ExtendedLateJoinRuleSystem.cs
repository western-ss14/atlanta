using Content.Server.Atlanta.GameTicking.Rules.Components;
using Content.Server.Atlanta.Player.Events;
using Content.Server.Atlanta.Waves.Events;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Shared.Actions;
using Content.Shared.Atlanta.Player.Components;
using Content.Shared.Atlanta.Player.Events;
using Content.Shared.Mind;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Atlanta.GameTicking.Rules;

/// <summary>
/// Handles events and try to spawn new entities with player attaching.
/// Powerful for others rules when you need some more logic for player spawning or respawn.
/// </summary>
public sealed class ExtendedLateJoinRuleSystem :  GameRuleSystem<ExtendedLateJoinRuleComponent>
{
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        // queue
        SubscribeLocalEvent<ExtendedSpawnActionsComponent, JoinLateGameQueueEvent>(OnJoinQueue);
        SubscribeLocalEvent<ExtendedSpawnActionsComponent, LeaveLateGameQueueEvent>(OnLeaveQueue);

        SubscribeLocalEvent<EnableQueueSpawningEvent>(OnQueueSpawningEnable);
        SubscribeLocalEvent<DisableQueueSpawningEvent>(OnQueueSpawningDisable);
        // spawners
        SubscribeLocalEvent<ExtendedLateJoinPointRegisterEvent>(OnSpawnerRegister);
        // spawning
        SubscribeLocalEvent<SpawnQueuePlayersEvent>(OnSpawnPlayersQueue);
        SubscribeLocalEvent<ExtendedSpawnEntityEvent>(OnExtendedSpawnEntity);

        _sawmill = _logManager.GetSawmill("Extended Late Join Rule");
    }

    private void OnQueueSpawningDisable(DisableQueueSpawningEvent ev)
    {
        var queue = EntityQueryEnumerator<ExtendedLateJoinRuleComponent>();
        while (queue.MoveNext(out var _, out var rule))
        {
            rule.Enabled = false;
        }
    }

    private void OnQueueSpawningEnable(EnableQueueSpawningEvent ev)
    {
        var queue = EntityQueryEnumerator<ExtendedLateJoinRuleComponent>();
        while (queue.MoveNext(out var _, out var rule))
        {
            rule.Enabled = true;
            rule.EntryMobProtoId = ev.MobProtoId;
            rule.EntrySpawnerProtoId = ev.SpawnerProtoId;
            RaiseLocalEvent(new SpawnQueuePlayersEvent(ev.MobProtoId, ev.SpawnerProtoId));
        }
    }

    private void OnSpawnerRegister(ExtendedLateJoinPointRegisterEvent ev)
    {
        var queue = EntityQueryEnumerator<ExtendedLateJoinRuleComponent>();
        while (queue.MoveNext(out var _, out var rule))
        {
            if (!rule.SpawnPointsDictionary.TryGetValue(ev.ProtoId, out var value))
            {
                value = [];
                rule.SpawnPointsDictionary[ev.ProtoId] = value;
            }

            value.Add(ev.SpawnerEnt);
        }
    }

    private void OnSpawnPlayersQueue(SpawnQueuePlayersEvent ev)
    {
        _sawmill.Debug("Starts player spawning!");

        var queue = EntityQueryEnumerator<ExtendedLateJoinRuleComponent>();
        List<EntityUid> removedMindsQueue = [];
        while (queue.MoveNext(out var _, out var rule))
        {
            if (rule.MindQueue.Count == 0)
            {
                _sawmill.Debug("Skip rule spawning because queue is empty.");
                continue;
            }

            foreach (var mindId in rule.MindQueue)
            {
                if (!TryComp<MindComponent>(mindId, out var mindComp))
                {
                    _sawmill.Debug($"Skip {mindId} mind because can't get mindComp");
                    continue;
                }

                if (!_mind.IsCharacterDeadIc(mindComp))
                {
                    _sawmill.Debug($"Skip {mindId} mind because player is not dead IC.");
                    continue;
                }

                if (TrySpawnPlayer(mindId, ev.PlayerProtoId, ev.SpawnPointProtoId, out var instance))
                {
                    removedMindsQueue.Add(mindId);
                }
            }

            foreach (var mindId in removedMindsQueue)
            {
                rule.MindQueue.Remove(mindId);
            }

            removedMindsQueue.Clear();
        }
    }

    private bool TrySpawnPlayer(EntityUid mindId, EntProtoId playerProtoId, EntProtoId spawnerProtoId, out EntityUid? instance)
    {
        if (!TryGetRandomSpawnerCoordinates(spawnerProtoId, out var coords))
        {
            instance = null;
            return false;
        }

        instance = Spawn(playerProtoId, coords!.Value);
        _mind.TransferTo(mindId, instance, true);

        RaiseLocalEvent(new PlayerSpawnAfterEvent(instance!.Value));
        return true;
    }

    private void OnLeaveQueue(Entity<ExtendedSpawnActionsComponent> ent, ref LeaveLateGameQueueEvent ev)
    {
        if (_mind.TryGetMind(ent.Owner, out var mindId , out var _))
        {
            var queue = EntityQueryEnumerator<ExtendedLateJoinRuleComponent>();
            while (queue.MoveNext(out var _, out var rule))
            {
                rule.MindQueue.Remove(mindId);

                _actions.RemoveAction(ent.Owner, ent.Comp.ActionId);
                ent.Comp.ActionId = null;
                _actions.AddAction(ent.Owner, ref ent.Comp.ActionId, ent.Comp.JoinProtoId);
            }
        }
        else
        {
            _sawmill.Debug("Cancel leaving queue because mind was not found.");
        }
    }

    private void OnJoinQueue(Entity<ExtendedSpawnActionsComponent> ent, ref JoinLateGameQueueEvent ev)
    {
        if (_mind.TryGetMind(ent.Owner, out var mindId , out var _))
        {
            var queue = EntityQueryEnumerator<ExtendedLateJoinRuleComponent>();
            while (queue.MoveNext(out var _, out var rule))
            {
                if (rule.Enabled)
                {
                    if (TrySpawnPlayer(mindId, rule.EntryMobProtoId, rule.EntrySpawnerProtoId, out _))
                        return;
                }
                rule.MindQueue.Add(mindId);
            }

            _actions.RemoveAction(ent.Owner, ent.Comp.ActionId);
            ent.Comp.ActionId = null;
            _actions.AddAction(ent.Owner, ref ent.Comp.ActionId, ent.Comp.LeaveProtoId);
        }
        else
        {

            _sawmill.Debug("Cancel joining queue because mind was not found.");
        }
    }

    #region NonPlayerEntity

    private void OnExtendedSpawnEntity(ref ExtendedSpawnEntityEvent ev)
    {
        if (TryGetRandomSpawnerCoordinates(ev.SpawnerProtoId, out var coords))
        {
            ev.Instance = Spawn(ev.EntityProtoId, coords!.Value);
        }
    }

    #endregion

    private bool TryGetRandomSpawnerCoordinates(EntProtoId pointProto, out MapCoordinates? coords)
    {
        var queue = EntityQueryEnumerator<ExtendedLateJoinRuleComponent>();
        while (queue.MoveNext(out var _, out var rule))
        {
            var spawner = _random.Pick(rule.SpawnPointsDictionary[pointProto]);
            coords = _transform.GetMapCoordinates(spawner);

            return true;
        }

        coords = null;

        _sawmill.Debug("Could not find any spawner. Maybe you forgot to place on a map?");
        return false;
    }
}
