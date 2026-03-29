using Content.Shared.Dataset;
using Robust.Shared.Prototypes;

namespace Content.Server.Atlanta.Mist;

[RegisterComponent]
public sealed partial class MistRadioComponent : Component
{
    [DataField(required: true)]
    public ProtoId<LocalizedDatasetPrototype> Pack { get; private set; }
}
