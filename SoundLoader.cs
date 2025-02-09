using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// サウンドリソースのロード,アンロードを担うクラス<para></para>
/// - Addressableを介してAudioClipを非同期にロード<para></para>
/// - ロード結果をキャッシュ管理クラス(ISoundCache)に委譲
/// </summary>
public class SoundLoader : ISoundLoader
{
    private readonly ISoundCache cache;

    public SoundLoader(ISoundCache cache)
    {
        this.cache = cache;
    }

    public async UniTask<AudioClip> LoadClip(string resourceAddress)
    {
        //ロード対象がキャッシュにあればそれを返す
        var cachedClip = cache.Retrieve(resourceAddress);
        if (cachedClip != null) return cachedClip;

        //対象リソースをロードして返す
        var handle = Addressables.LoadAssetAsync<AudioClip>(resourceAddress);
        var clip = await handle.Task;
        if (clip != null)
        {
            cache.Add(resourceAddress, clip);
            return clip;
        }

        throw new InvalidOperationException($"SoundLoader: リソース '{resourceAddress}' のロードに失敗");
    }

    public void UnloadClip(string resourceAddress)
    {
        cache.Remove(resourceAddress);
    }
}