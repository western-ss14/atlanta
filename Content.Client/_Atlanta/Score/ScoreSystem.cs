using Content.Client.Lobby.UI;
using Content.Shared.Score;
using Robust.Client.UserInterface;

namespace Content.Client.Atlanta.Score;

/// <summary>
/// Receives scoreboard data from the server and updates the lobby UI.
/// </summary>
public sealed class ScoreSystem : EntitySystem
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<LoadedScoreListEvent>(OnScoreList);
    }

    private void OnScoreList(LoadedScoreListEvent ev)
    {
        if (_userInterfaceManager.ActiveScreen is LobbyGui lobbyGui)
        {
            lobbyGui.UpdateScoreList(ev.Scores);
        }
    }

    public void LoadScoreboard()
    {
        RaiseNetworkEvent(new RequestScoreListEvent());
    }
}
