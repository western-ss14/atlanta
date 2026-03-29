using Content.Shared.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Client.Audio;

public sealed partial class ClientGlobalSoundSystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private readonly Dictionary<GlobalEventMusicType, EntityUid?> _globalEventAudio = new(1);
    private readonly Dictionary<GlobalEventMusicType, (ResolvedSoundSpecifier specifier, TimeSpan length, TimeSpan startTime, AudioParams? audioParams)> _loopEventAudio = new(1);

    private static float _volumeSlider;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var keys = _loopEventAudio.Keys;

        foreach (var key in keys)
        {
            var loopAudio = _loopEventAudio[key];

            if (_gameTiming.RealTime - loopAudio.startTime > loopAudio.length)
            {
                if (_globalEventAudio.TryGetValue(key, out var oldStream))
                    _audio.Stop(oldStream);


                var audioParams = (loopAudio.audioParams ?? AudioParams.Default).WithVolume(_volumeSlider);

                var stream = _audio.PlayGlobal(loopAudio.specifier, Filter.Local(), false, audioParams);

                _globalEventAudio.Remove(key);
                if (stream != null)
                    _globalEventAudio.Add(key, stream.Value.Entity);

                _loopEventAudio[key] = (loopAudio.specifier, _audio.GetAudioLength(loopAudio.specifier),
                    _gameTiming.RealTime, audioParams);
            }
        }
    }

    private void PlayGlobalEventMusic(GlobalEventMusicEvent ev)
    {
        if(!_eventAudioEnabled || _globalEventAudio.ContainsKey(ev.Type))
            return;

        var audioParams = (ev.AudioParams ?? AudioParams.Default).WithVolume(_volumeSlider);

        var stream = _audio.PlayGlobal(ev.Specifier, Filter.Local(), false, audioParams);
        if (stream != null)
            _globalEventAudio.Add(ev.Type, stream.Value.Entity);

        if (ev.Loop)
        {
            _loopEventAudio.Add(ev.Type,
                (ev.Specifier, _audio.GetAudioLength(ev.Specifier), _gameTiming.RealTime, audioParams));
        }
    }

    private void StopGlobalEventMusic(StopGlobalEventMusic ev)
    {
        if (!_globalEventAudio.TryGetValue(ev.Type, out var stream))
            return;

        _audio.Stop(stream);

        _globalEventAudio.Remove(ev.Type);
        _loopEventAudio.Remove(ev.Type);
    }

    private void AmbienceCVarChanged(float obj)
    {
        _volumeSlider = SharedAudioSystem.GainToVolume(obj);

        foreach (var audio in _globalEventAudio.Values)
        {
            _audio.SetVolume(audio, _volumeSlider);
        }
    }
}
