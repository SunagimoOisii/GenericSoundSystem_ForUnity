using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Audio;
using System.Threading;
using System.Linq;

/// <summary>
/// SoundSystemが操作するクラスの1つ<para></para>
/// - AudioSourceをプール管理し、効率的にSEを再生<para></para>
/// - 未使用AudioSourceの定期クリーンアップを行う<para></para>
/// </summary>
public sealed class SEManager
{
    private readonly ISoundLoader loader;

    private readonly GameObject sourceParentObj;
    private Queue<AudioSource> audioSourcePool;
    private readonly Dictionary<AudioSource, float> lastUsedTimeDict;
    private readonly CancellationTokenSource cleanupTokenSource;
    private readonly int maxPoolSize;

    public SEManager(AudioMixerGroup mixerGroup, ISoundLoader loader,
        int initialPoolSize, int maxPoolSize = 10, float sourceRemoveTime = 30f, float unusedLifetime = 30f)
    {
        this.loader = loader;

        //SE用AudioSourceを持ったGameObjectの親オブジェクトを生成
        sourceParentObj = new("SE_AudioSources");

        this.maxPoolSize = maxPoolSize;
        audioSourcePool = new();
        lastUsedTimeDict = new();

        //AudioSourceプールの初期化
        for (int i = 0; i < initialPoolSize; i++)
        {
            var source = CreateAudioSource(mixerGroup, $"SESource_{i}");
            audioSourcePool.Enqueue(source);
            lastUsedTimeDict[source] = Time.time;
        }

        //一定時間未使用のAudioSourceを削除する処理を作動させる
        cleanupTokenSource = new();
        CleanupPeriodically(cleanupTokenSource.Token, sourceRemoveTime, unusedLifetime).Forget();
    }

    /// <summary>
    /// 未使用のAudioSourceを削除する
    /// </summary>
    /// <param name="interval">削除を実行する間隔(秒)</param>
    /// <param name="unusedLifeTime">いつから削除対象になるか(秒)</param>
    private async UniTask CleanupPeriodically(CancellationToken token,
        float interval, float unusedLifetime)
    {
        while (token.IsCancellationRequested == false)
        {
            CleanupUnusedAudioSources(unusedLifetime);
            await UniTask.Delay((int)(interval * 1000), cancellationToken: token);
        }
    }

    private void CleanupUnusedAudioSources(float unusedLifeTime)
    {
        var currentTime = Time.time;
        var toRemove = new List<AudioSource>();
        foreach (var entry in lastUsedTimeDict)
        {
            if (currentTime - entry.Value > unusedLifeTime)
            {
                toRemove.Add(entry.Key);
            }
        }

        //指定時間の間未使用のAudioSource(とそれがアタッチされたGameObject)を削除
        foreach (var source in toRemove)
        {
            audioSourcePool = new Queue<AudioSource>(audioSourcePool.Where(s => s != source));
            lastUsedTimeDict.Remove(source);
            Object.Destroy(source.gameObject);
        }
    }

    /// <summary>
    /// 指定アドレスのAudioClipをロードし、再生を開始する
    /// </summary>
    /// <param name="volume">音量(範囲: 0.0～1.0)</param>
    /// <param name="pitch">音程(範囲: 0.0～1.0)</param>
    /// <param name="volume">サラウンド度合い(範囲: 0.0～1.0)</param>
    public async UniTask Play(string resourceAddress, Vector3 position = default,
        float volume = 0.5f, float pitch = 1.0f, float spatialBlend = 1.0f)
    {
        //サウンドリソースのロード
        var clip = await loader.LoadClip(resourceAddress);
        if (clip == null)
        {
            Debug.LogWarning($"SEManager: 指定リソース '{resourceAddress}' が見つからない");
            return;
        }

        //再生音声の設定
        var source = RetrieveAvailableAudioSource();
        source.pitch              = pitch;
        source.volume             = volume;
        source.spatialBlend       = spatialBlend;
        source.transform.position = position;
        source.PlayOneShot(clip);

        //AudioSourceの使用時間を記録
        lastUsedTimeDict[source] = Time.time;
    }

    private AudioSource RetrieveAvailableAudioSource()
    {
        //プール内で再生中でないAudioSourceがあれば、それを返す
        foreach (var source in audioSourcePool)
        {
            if (source != null && 
                source.isPlaying == false)
            {
                lastUsedTimeDict[source] = Time.time;
                return source;
            }
        }

        //プールが空の場合やすべて削除済みの場合
        if (audioSourcePool.Count == 0 || 
            audioSourcePool.Peek() == null)
        {
            Debug.LogWarning("SEManager: AudioSourceプールが空か無効な参照を含んでいるため新規作成する");
            var newS = CreateAudioSource(null, $"SESource_{audioSourcePool.Count}");
            audioSourcePool.Enqueue(newS);
            return newS;
        }

        //プールが最大サイズに達している場合
        if (audioSourcePool.Count >= maxPoolSize)
        {
            var oldestSource = audioSourcePool.Dequeue();

            if (oldestSource == null)
            {
                Debug.LogWarning("SEManager: 再利用対象のAudioSourceが無効のため新規作成する");
                var newS = CreateAudioSource(audioSourcePool.Peek()?.outputAudioMixerGroup, $"SESource_{audioSourcePool.Count}");
                audioSourcePool.Enqueue(newS);
                return newS;
            }

            audioSourcePool.Enqueue(oldestSource);
            lastUsedTimeDict[oldestSource] = Time.time;
            return oldestSource;
        }

        //プールが最大サイズに達していない場合
        var newSource = CreateAudioSource(audioSourcePool.Peek().outputAudioMixerGroup, $"SESource_{audioSourcePool.Count}");
        audioSourcePool.Enqueue(newSource);
        lastUsedTimeDict[newSource] = Time.time;
        return newSource;
    }

    /// <summary>
    /// AudioSourceをプールから取得し、必要に応じて新しいAudioSourceを作成<para></para>
    /// 再利用可能なAudioSourceが存在しない場合は、最古のAudioSourceを再利用する
    /// </summary>
    private AudioSource CreateAudioSource(AudioMixerGroup mixerGroup, string name)
    {
        var sourceObj = new GameObject(name);
        sourceObj.transform.parent = sourceParentObj.transform;

        var source = sourceObj.AddComponent<AudioSource>();
        source.playOnAwake           = false;
        source.outputAudioMixerGroup = mixerGroup;
        return source;
    }
}