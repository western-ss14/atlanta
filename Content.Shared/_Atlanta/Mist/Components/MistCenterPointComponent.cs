namespace Content.Shared.Atlanta.Mist.Components;

/// <summary>
/// This is used for find the center point of the mist.
/// </summary>
[RegisterComponent]
public sealed partial class MistCenterPointComponent : Component
{
    [DataField]
    public float Distance = 100f;

    [DataField]
    public float DeathDistance = 120f;
}
