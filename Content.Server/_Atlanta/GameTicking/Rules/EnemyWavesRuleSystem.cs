using System.Linq;
using Content.Server.Atlanta.GameTicking.Rules.Components;
using Content.Server.Atlanta.Player;
using Content.Server.Atlanta.Waves;
using Content.Server.Atlanta.Waves.Component;
using Content.Server.Atlanta.Waves.Events;
using Content.Server.GameTicking.Rules;
using Content.Shared.Atlanta.Waves;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Atlanta.GameTicking.Rules;

/// <summary>
/// Uses for spawn enemies waves.
/// </summary>
public sealed class EnemyWavesRuleSystem :  GameRuleSystem<EnemyWavesRuleComponent>
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;
    public override void Initialize()
    {
        base.Initialize();

        // rule
        SubscribeLocalEvent<EnemyWavesRuleComponent, ComponentInit>(OnRuleComponentInit);

        SubscribeLocalEvent<PauseWavesEvent>(OnPauseWaves);
        SubscribeLocalEvent<UnpauseWavesEvent>(OnUnpauseWaves);

        SubscribeLocalEvent<DelayWaveEvent>(OnDelayWaves);
        SubscribeLocalEvent<SpeedupWaveEvent>(OnSpeedupWaves);

        SubscribeLocalEvent<ChangeDifficultyEvent>(OnChangeDifficulty);
        SubscribeLocalEvent<MultiplyDifficultyEvent>(OnMultiplyDifficulty);
        // enemy controlling
        SubscribeLocalEvent<EnemyDeadReportEvent>(OnEnemyDeadReport);

        _sawmill = _logManager.GetSawmill("Enemy Waves Rule");
    }

    private readonly List<(EntityUid, TimeSpan)> _deletedEntries = [];

    #region Component
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var queue = EntityQueryEnumerator<EnemyWavesRuleComponent>();
        while (queue.MoveNext(out var rule))
        {
            // waves timing
            if (!rule.IsPaused && _timing.CurTime > rule.NextWaveTime)
            {
                SpawnEnemies(rule);

                rule.NextWaveTime = _timing.CurTime + rule.WavesTimings;
            }

            // deleting enemies
            if (rule.QueuedToDeleteEnemies.Count == 0)
                continue;

            foreach (var entry in rule.QueuedToDeleteEnemies.Where(entry => entry.DeletionTime < _timing.CurTime))
            {
                QueueDel(entry.Enemy);
                _deletedEntries.Add(entry);
            }

            foreach (var entry in _deletedEntries)
            {
                rule.QueuedToDeleteEnemies.Remove(entry);
            }

            _deletedEntries.Clear();
        }
    }
    private void OnRuleComponentInit(Entity<EnemyWavesRuleComponent> ent, ref ComponentInit args)
    {
        ent.Comp.NextWaveTime = _timing.CurTime + ent.Comp.WavesTimings;
    }
#endregion

    #region WavesDifficulty
    private void OnChangeDifficulty(ChangeDifficultyEvent ev)
    {
        var queue = EntityQueryEnumerator<EnemyWavesRuleComponent>();
        while (queue.MoveNext(out var rule))
        {
            rule.Difficulty += ev.Difficulty;
        }
    }


    private void OnMultiplyDifficulty(MultiplyDifficultyEvent ev)
    {
        var queue = EntityQueryEnumerator<EnemyWavesRuleComponent>();
        while (queue.MoveNext(out var rule))
        {
            rule.Difficulty *= ev.DifficultyRatio;
        }
    }
    #endregion

    #region WavesTiming
    private void OnSpeedupWaves(SpeedupWaveEvent ev)
    {
        var queue = EntityQueryEnumerator<EnemyWavesRuleComponent>();
        while (queue.MoveNext(out var rule))
        {
            rule.WavesTimings -= ev.SpeedUp;
        }
    }

    private void OnDelayWaves(DelayWaveEvent ev)
    {
        var queue = EntityQueryEnumerator<EnemyWavesRuleComponent>();
        while (queue.MoveNext(out var rule))
        {
            rule.WavesTimings += ev.Delay;
        }
    }
    #endregion

    #region WavesLifetime

    private void OnUnpauseWaves(UnpauseWavesEvent ev)
    {
        var queue = EntityQueryEnumerator<EnemyWavesRuleComponent>();
        while (queue.MoveNext(out var rule))
        {
            rule.IsPaused = false;
            rule.NextWaveTime += _timing.CurTime - rule.PauseTime;
        }
    }

    private void OnPauseWaves(PauseWavesEvent ev)
    {
        var queue = EntityQueryEnumerator<EnemyWavesRuleComponent>();
        while (queue.MoveNext(out var rule))
        {
            rule.IsPaused = true;
            rule.PauseTime = _timing.CurTime;
        }
    }
    #endregion

    #region Enemies
    private void SpawnEnemies(EnemyWavesRuleComponent rule)
    {
        if (!_prototypeManager.TryIndex(rule.EnemyStackPrototype, out var enemyStackPrototype))
        {
            _sawmill.Error("Could not index " + rule.EnemyStackPrototype + " enemy stack prototype.");
            return;
        }

        float currentDifficulty = 0;

        while (currentDifficulty < rule.Difficulty)
        {
            var enemyProtoId = _random.Pick(enemyStackPrototype.Weights);

            if (!_prototypeManager.TryIndex<EnemyPrototype>(enemyProtoId, out var enemyPrototype))
            {
                _sawmill.Error("Could not index " + enemyProtoId + " enemy prototype.");
                continue;
            }

            var ev = new ExtendedSpawnEntityEvent(enemyPrototype.ProtoId,
                rule.EnemySpawnPointPrototype);
            RaiseLocalEvent(ref ev);

            if (ev.Instance == null)
                continue;

            EnsureComp<LifeTrackerComponent>(ev.Instance.Value);
            EnsureComp<WaveEnemyComponent>(ev.Instance.Value);

            rule.AliveEnemies.Add(ev.Instance.Value);

            currentDifficulty += enemyPrototype.Difficulty;
        }

        RaiseLocalEvent(new WaveStartsEvent());
    }

    private void OnEnemyDeadReport(EnemyDeadReportEvent ev)
    {
        var queue = EntityQueryEnumerator<EnemyWavesRuleComponent>();
        while (queue.MoveNext(out var rule))
        {
            rule.AliveEnemies.Remove(ev.Enemy);
            rule.QueuedToDeleteEnemies.Add((ev.Enemy, ev.RemovingTime));

            if (rule.AliveEnemies.Count != 0)
                continue;

            var enemiesDeadEv = new AllWaveEnemiesDead();
            RaiseLocalEvent(enemiesDeadEv);
        }
    }
    #endregion
}
