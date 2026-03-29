namespace Content.Server.Atlanta.Supply.Events;

public sealed class SpawnSupplyEvent(string category)
{
    public readonly string Category = category;
}
