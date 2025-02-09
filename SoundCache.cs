using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// サウンドリソースのキャッシュ管理を担うクラス<para></para>
/// - AudioClipをキーとしてキャッシュし、再利用可能に<para></para>
/// - アクセス時間に基づく未使用リソースのクリーンアップが可能<para></para>
/// - Addressablesを介したリソース解放機能を提供
/// </summary>
public class SoundCache : ISoundCache
{
    private readonly Dictionary<string, AudioClip> cache = new();
    private readonly Dictionary<string, float> lastAccessTime = new();

    /// <summary>
    /// 指定リソースをキャッシュから取得する<para></para>
    /// 取得時に最終アクセス時間も更新する
    /// </summary>
    public AudioClip Retrieve(string resourceAddress)
    {
        if (cache.TryGetValue(resourceAddress, out var clip))
        {
            UpdateAccessTime(resourceAddress);
            return clip;
        }
        return null;
    }

    /// <summary>
    /// 指定リソースをキャッシュに追加する<para></para>
    /// 取得時に最終アクセス時間も更新する
    /// </summary>
    public void Add(string resourceAddress, AudioClip clip)
    {
        cache[resourceAddress] = clip;
        UpdateAccessTime(resourceAddress);
    }

    private void UpdateAccessTime(string resourceAddress)
    {
        lastAccessTime[resourceAddress] = Time.time;
    }

    public void Remove(string resourceAddress)
    {
        if (cache.TryGetValue(resourceAddress, out var clip))
        {
            Addressables.Release(clip);
            cache.Remove(resourceAddress);
            lastAccessTime.Remove(resourceAddress);
        }
    }

    public void CleanupUnused(float idleTimeThreshold)
    {
        var currentTime = Time.time;
        var toRemove = new List<string>();

        foreach (var entry in lastAccessTime)
        {
            if (currentTime - entry.Value > idleTimeThreshold)
            {
                toRemove.Add(entry.Key);
            }
        }

        foreach (var key in toRemove)
        {
            Remove(key);
        }
    }

    public void Clear()
    {
        foreach (var clip in cache.Values)
        {
            Addressables.Release(clip);
        }
        cache.Clear();
        lastAccessTime.Clear();
    }
}