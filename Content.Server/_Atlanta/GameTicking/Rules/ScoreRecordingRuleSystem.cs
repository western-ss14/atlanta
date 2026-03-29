using Content.Server.Atlanta.GameTicking.Rules.Components;
using Content.Server.Atlanta.Score.Events;
using Content.Server.GameTicking.Rules;

namespace Content.Server.Atlanta.GameTicking.Rules;

/// <summary>
/// This handles...
/// </summary>
public sealed class ScoreRecordingRuleSystem : GameRuleSystem<ScoreRecordingRuleComponent>
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MakeScoreRecordEvent>(OnRecordingScore);
        SubscribeLocalEvent<RequireScoreRecordEvent>(OnRequireScoreRecord);
        SubscribeLocalEvent<RequireScoreRecordsEvent>(OnRequireScoreRecords);
    }

    private void OnRequireScoreRecords(ref RequireScoreRecordsEvent args)
    {
        var query = EntityQueryEnumerator<ScoreRecordingRuleComponent>();
        while (query.MoveNext(out _, out var comp))
        {
            foreach (var key in comp.PlayersRecords.Keys)
            {
                var record = comp.PlayersRecords[key];
                args.ScoreRecords.Add((key, record.Item1, record.Item2));
            }
        }
    }

    private void OnRequireScoreRecord(ref RequireScoreRecordEvent args)
    {
        var query = EntityQueryEnumerator<ScoreRecordingRuleComponent>();
        while (query.MoveNext(out _, out var comp))
        {
            if (!comp.PlayersRecords.TryGetValue(args.UserId, out var record))
                return;

            args.WinScore = record.Item1;
            args.Kills = record.Item2;
        }
    }

    private void OnRecordingScore(ref MakeScoreRecordEvent args)
    {
        var query = EntityQueryEnumerator<ScoreRecordingRuleComponent>();
        while (query.MoveNext(out _, out var comp))
        {
            var winScore = 0;
            var kills = 0;

            if (comp.PlayersRecords.TryGetValue(args.UserId, out var entry))
            {
                winScore += entry.Item1;
                kills += entry.Item2;
            }

            switch (args.ScoreRecordType)
            {
                case ScoreRecordType.WinScore:
                    winScore += args.Count;
                    break;
                case ScoreRecordType.Kill:
                    kills += args.Count;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            comp.PlayersRecords[args.UserId] = (winScore, kills);
        }
    }
}
