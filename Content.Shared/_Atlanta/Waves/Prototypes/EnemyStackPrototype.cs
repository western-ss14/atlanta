using Content.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atlanta.Waves;

/// <summary>
/// This is a prototype for set up enemy stacks waves for <see cref="EnemyWavesRuleSystem"/>
/// </summary>
[Prototype("wavesEnemyStack")]
public sealed partial class EnemyStackPrototype : IWeightedRandomPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public Dictionary<string, float> Weights { get; private set; } = [];

    [DataField("timing")]
    public TimeSpan WavesTiming = TimeSpan.FromMinutes(3);

    public bool IsPaused = false;
}
