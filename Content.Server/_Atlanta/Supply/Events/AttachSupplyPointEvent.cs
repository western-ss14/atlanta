namespace Content.Server.Atlanta.Supply.Events;

public sealed class AttachSupplyPointEvent(string category, EntityUid target)
{
    /// <summary>
    /// Uses to have access by name.
    /// </summary>
    public readonly string Category = category;
    /// <summary>
    /// Target point that will be saved with category name.
    /// </summary>
    public readonly EntityUid Target = target;
}
