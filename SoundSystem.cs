using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// サウンド管理のエントリーポイントを提供するクラス<para></para>
/// - 各マネージャと機能インターフェースを外部から受け取り統一的に管理<para></para>
/// - 本モジュールに同梱のAudioMixerオブジェクトの使用を前提としている
/// </summary>
public sealed class SoundSystem
{
    public BGMManager BGM { get; }
    public SEManager SE { get; }
    public ListenerEffector Effector { get; }
    public ISoundLoader Loader { get; }
    public ISoundCache Cache { get; }

    private readonly AudioMixer mixer;

    public SoundSystem(ISoundCache cache, ISoundLoader loader,
        AudioMixer mixer, AudioMixerGroup bgmGroup, AudioMixerGroup seGroup, int sePoolSize)
    {
        this.mixer = mixer;

        Cache    = cache;
        Loader   = loader;
        BGM      = new(bgmGroup, Loader);
        SE       = new(seGroup, Loader, sePoolSize);
        Effector = new();
    }

    public float? GetMixerParameter(string exposedParamName)
    {
        if (mixer.GetFloat(exposedParamName, out float value))
        {
            return value;
        }
        else
        {
            Debug.LogWarning($"SoundSystem: パラメータ '{exposedParamName}' の取得に失敗");
            return null;
        }
    }

    public void SetMixerParameter(string exposedParamName, float value)
    {
        if (mixer.SetFloat(exposedParamName, value) == false)
        {
            Debug.LogWarning($"SoundSystem: パラメータ '{exposedParamName}' の設定に失敗");
        }
    }
}