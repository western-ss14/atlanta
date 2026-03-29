using Content.Server.Atlanta.Waves;
using Content.Shared.Atlanta.Waves;
using Robust.Shared.Prototypes;

namespace Content.Server.Atlanta.GameTicking.Rules.Components;

/// <summary>
/// Uses for spawn enemies waves.
/// </summary>
[RegisterComponent, Access(typeof(EnemyWavesRuleSystem))]
public sealed partial class EnemyWavesRuleComponent : Component
{
    [DataField("enemyStack", required: true)]
    public ProtoId<EnemyStackPrototype> EnemyStackPrototype = default!;

    [DataField("enemySpawnPoint", required: true)]
    public EntProtoId EnemySpawnPointPrototype = default!;

    [DataField]
    public List<EntityUid> AliveEnemies = [];

    [DataField]
    public List<(EntityUid Enemy, TimeSpan DeletionTime)> QueuedToDeleteEnemies = [];

    [DataField(required: true)]
    public float Difficulty = default!;

    [DataField]
    public bool IsPaused = false;

    [DataField]
    public TimeSpan PauseTime = default!;

    /// <summary>
    /// Time between waves
    /// </summary>
    [DataField]
    public TimeSpan WavesTimings = TimeSpan.FromMinutes(2);

    /// <summary>
    /// Time before next wave
    /// </summary>
    [DataField]
    public TimeSpan NextWaveTime;
}
