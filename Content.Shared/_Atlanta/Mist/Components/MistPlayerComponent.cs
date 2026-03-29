using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Atlanta.Mist.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MistPlayerComponent : Component
{
    [AutoNetworkedField]
    [DataField]
    public float MistLevel = 0.15f;

    [AutoNetworkedField]
    [DataField]
    public float MinMistLevel = 0.15f;

    [AutoNetworkedField]
    [DataField]
    public MapCoordinates? Center = default!;

    /// <summary>
    /// Contains distance from center after that the mist overlay starts to be more visible.
    /// </summary>
    [AutoNetworkedField]
    [DataField]
    public float ToleranceDistance = default!;

    /// <summary>
    /// If player distance from center greater than it, them will die.
    /// </summary>
    [DataField]
    public float DeathDistance = default!;
}
