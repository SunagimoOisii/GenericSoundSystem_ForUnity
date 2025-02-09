using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// �T�E���h���\�[�X�̃L���b�V���Ǘ���S���N���X<para></para>
/// - AudioClip���L�[�Ƃ��ăL���b�V�����A�ė��p�\��<para></para>
/// - �A�N�Z�X���ԂɊ�Â����g�p���\�[�X�̃N���[���A�b�v���\<para></para>
/// - Addressables��������\�[�X����@�\���
/// </summary>
public class SoundCache : ISoundCache
{
    private readonly Dictionary<string, AudioClip> cache = new();
    private readonly Dictionary<string, float> lastAccessTime = new();

    /// <summary>
    /// �w�胊�\�[�X���L���b�V������擾����<para></para>
    /// �擾���ɍŏI�A�N�Z�X���Ԃ��X�V����
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
    /// �w�胊�\�[�X���L���b�V���ɒǉ�����<para></para>
    /// �擾���ɍŏI�A�N�Z�X���Ԃ��X�V����
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