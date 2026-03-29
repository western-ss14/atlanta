using Robust.Shared.GameStates;

namespace Content.Shared.Roles.Components;

/// <summary>
/// Tags a mind role entity as a Royal Battle fighter.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RbPlayerRoleComponent : BaseMindRoleComponent;
