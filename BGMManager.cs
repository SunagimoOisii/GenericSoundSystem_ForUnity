using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Audio;
using System.Threading;

/// <summary>
/// BGMの再生、停止、フェード機能を提供するクラス<para></para>
/// </summary>
public sealed class BGMManager
{
    private readonly ISoundLoader loader;

    private readonly GameObject sourceParentObj = null;
    private (AudioSource active, AudioSource inactive) bgmSources;

    private CancellationTokenSource fadeCancellationToken;
    private string currentBGMAddress;

    public BGMManager(AudioMixerGroup mixerGroup, ISoundLoader loader)
    {
        this.loader = loader;

        sourceParentObj = new("BGM_AudioSources");

        //BGM用AudioSourceを生成
        var source1 = CreateAudioSourceObj(mixerGroup, "BGMSource_0");
        var source2 = CreateAudioSourceObj(mixerGroup, "BGMSource_1");
        bgmSources = (source1, source2);
    }

    private AudioSource CreateAudioSourceObj(AudioMixerGroup mixerGroup, string name)
    {   
        var sourceObj = new GameObject(name);
        sourceObj.transform.parent = sourceParentObj.transform;

        var source = sourceObj.AddComponent<AudioSource>();
        source.loop                  = true;
        source.playOnAwake           = false;
        source.outputAudioMixerGroup = mixerGroup;
        return source;
    }

    /// <summary>
    /// 指定アドレスのAudioClipをロードし、再生を開始する
    /// </summary>
    /// <param name="volume">音量(範囲: 0.0～1.0)</param>
    public async UniTask Play(string resourceAddress, float volume = 0.5f)
    {
        var clip = await loader.LoadClip(resourceAddress);
        if (clip == null) return;

        bgmSources.active.clip   = clip;
        bgmSources.active.volume = volume;
        bgmSources.active.Play();
    }

    public void Stop()
    {
        bgmSources.active.Stop();
        bgmSources.active.clip = null;
    }

    private async UniTask ExecuteFade(float duration, float startVolume, float targetVolume)
    {
        //既にフェード処理が行われていた場合は上書き
        fadeCancellationToken?.Cancel();
        fadeCancellationToken = new();
        var token = fadeCancellationToken.Token;

        try
        {
            //フェード実行
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                if (token.IsCancellationRequested) return;

                float progress  = elapsedTime / duration;
                float newVolume = Mathf.Lerp(startVolume, targetVolume, progress);
                bgmSources.active.volume = newVolume;

                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }

            bgmSources.active.volume = targetVolume;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("BGMManager: フェード処理を中断");
        }
    }

    public async UniTask FadeIn(string resourceAddress, float duration, float volume = 1.0f)
    {
        var clip = await loader.LoadClip(resourceAddress);
        if (clip == null) return;

        bgmSources.active.clip = clip;
        bgmSources.active.volume = 0;
        bgmSources.active.Play();

        await ExecuteFade(duration, 0.0f, volume);
    }

    public async UniTask FadeOut(float duration)
    {
        await ExecuteFade(duration, bgmSources.active.volume, 0.0f);
        bgmSources.active.Stop();
        bgmSources.active.clip = null;
    }

    /// <summary>
    /// 現在再生中のBGMから別のBGMにクロスフェード<para></para>
    /// 再生中BGMと同じモノを指定した場合何も行われない
    /// </summary>
    /// <param name="duration">クロスフェード完了までの時間(秒)</param>
    public async UniTask CrossFade(string resourceAddress, float duration)
    {
        if (resourceAddress == currentBGMAddress)
        {
            Debug.LogWarning($"BGMManager: 同BGM '{resourceAddress}' が指定されたため、クロスフェードを中止");
            return;
        }

        //既にフェード処理が行われていた場合は上書き
        fadeCancellationToken?.Cancel();
        fadeCancellationToken = new();
        var token = fadeCancellationToken.Token;

        try
        {
            var newActiveSource = bgmSources.inactive;
            var oldActiveSource = bgmSources.active;

            var clip = await loader.LoadClip(resourceAddress);
            if (clip == null) return;

            newActiveSource.clip = clip;
            newActiveSource.volume = 0;
            newActiveSource.Play();

            //クロスフェード実行
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                if (token.IsCancellationRequested) return;

                float progress = elapsedTime / duration;
                oldActiveSource.volume = Mathf.Lerp(1.0f, 0.0f, progress);
                newActiveSource.volume = Mathf.Lerp(0.0f, 1.0f, progress);
                elapsedTime += Time.deltaTime;
                await UniTask.Yield();
            }

            oldActiveSource.Stop();
            bgmSources = (newActiveSource, oldActiveSource);

            //再生中のリソースアドレスを更新
            currentBGMAddress = resourceAddress;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("BGMManager: クロスフェード処理を中断");
        }
    }
}