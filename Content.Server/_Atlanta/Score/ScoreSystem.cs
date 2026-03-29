using System.Threading.Tasks;
using Content.Server.Atlanta.GameTicking.Rules.Components;
using Content.Server.Atlanta.Score.Events;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Shared.Score;
using Robust.Shared.Network;

namespace Content.Server.Atlanta.Score;

/// <summary>
/// This handles...
/// </summary>
public sealed class ScoreSystem : EntitySystem
{
    public static readonly string ScoreRecordingRule = "ScoreRecordingRule";

    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        SubscribeNetworkEvent<RequestScoreListEvent>(OnScoreListRequired);

        _sawmill = _logManager.GetSawmill("Score");
    }

    /// <summary>
    /// Set player score directly.
    /// </summary>
    /// <param name="userId">Player NetUserId</param>
    /// <param name="winScore">Wining player score</param>
    /// <param name="kills">Count of kills</param>
    public Task UpdatePlayerScore(NetUserId userId, int winScore, int kills)
    {
        return _db.SavePlayerScore(userId, winScore, kills);
    }

    /// <summary>
    /// Add to current values winScore and kills, saves result to _db.
    /// Not check for negative values.
    /// </summary>
    /// <param name="userId">Player NetUserId</param>
    /// <param name="winScore">Wining player score</param>
    /// <param name="kills">Count of kills</param>
    public async Task IncreasePlayerScore(NetUserId userId, int winScore, int kills)
    {
        var score = await _db.LoadPlayerScore(userId);

        var currentWinScore = score?.Item2;
        var currentKills = score?.Item3;

        await UpdatePlayerScore(userId, (currentWinScore ?? 0) + winScore, (currentKills ?? 0) + kills);
    }

    public void InitializeScoreRecording()
    {
        var gameTicker = _entityManager.System<GameTicker>();
        if (gameTicker.IsGameRuleActive<ScoreRecordingRuleComponent>())
            return;

        gameTicker.StartGameRule(ScoreRecordingRule);
    }

    public void RecordWinScore(NetUserId userId, int winScore)
    {
        var ev = new MakeScoreRecordEvent(userId, ScoreRecordType.WinScore, winScore);
        RaiseLocalEvent(ref ev);
    }

    public void RecordKills(NetUserId userId, int kills)
    {
        var ev = new MakeScoreRecordEvent(userId, ScoreRecordType.Kill, kills);
        RaiseLocalEvent(ref ev);
    }

    public async void UploadPlayerScoreRecords(NetUserId userId)
    {
        var record = LoadPlayerScoreRecord(userId);

        await IncreasePlayerScore(userId, record.Item1, record.Item2);
    }

    public void UploadPlayersScoreRecords()
    {
        var ev = new RequireScoreRecordsEvent();
        RaiseLocalEvent(ref ev);

        foreach (var entry in ev.ScoreRecords)
        {
            UploadPlayerScoreRecords(entry.Item1);
        }
    }

    public (int WinScore, int Kills) LoadPlayerScoreRecord(NetUserId userId)
    {
        var ev = new RequireScoreRecordEvent(userId);
        RaiseLocalEvent(ref ev);

        return (ev.WinScore, ev.Kills);
    }

    private async void OnScoreListRequired(RequestScoreListEvent ev, EntitySessionEventArgs args)
    {
        _sawmill.Debug($"{args.SenderSession} required score list.");
        var list = await _db.LoadPlayersScores();
        var rev = new LoadedScoreListEvent(list);
        _sawmill.Debug($"Score list successful sent to {args.SenderSession}.");
        RaiseNetworkEvent(rev, args.SenderSession);
    }
}
