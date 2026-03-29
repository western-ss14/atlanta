using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Atlanta.Mist;

public sealed class MistRadioSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NewMistRadioAnnounceEvent>(OnMistRadioAnnounce);
    }

    private void OnMistRadioAnnounce(NewMistRadioAnnounceEvent ev)
    {
        var queue = EntityQueryEnumerator<MistRadioComponent>();

        while (queue.MoveNext(out var uid, out var comp))
        {
            if (!_prototypeManager.TryIndex(comp.Pack, out var messagePack))
                continue;

            _chat.TrySendInGameICMessage(uid, Loc.GetString(_random.Pick(messagePack.Values)), InGameICChatType.Speak, ChatTransmitRange.Normal);
        }
    }
}
