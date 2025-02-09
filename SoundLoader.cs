using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// �T�E���h���\�[�X�̃��[�h,�A�����[�h��S���N���X<para></para>
/// - Addressable�����AudioClip��񓯊��Ƀ��[�h<para></para>
/// - ���[�h���ʂ��L���b�V���Ǘ��N���X(ISoundCache)�ɈϏ�
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
        //���[�h�Ώۂ��L���b�V���ɂ���΂����Ԃ�
        var cachedClip = cache.Retrieve(resourceAddress);
        if (cachedClip != null) return cachedClip;

        //�Ώۃ��\�[�X�����[�h���ĕԂ�
        var handle = Addressables.LoadAssetAsync<AudioClip>(resourceAddress);
        var clip = await handle.Task;
        if (clip != null)
        {
            cache.Add(resourceAddress, clip);
            return clip;
        }

        throw new InvalidOperationException($"SoundLoader: ���\�[�X '{resourceAddress}' �̃��[�h�Ɏ��s");
    }

    public void UnloadClip(string resourceAddress)
    {
        cache.Remove(resourceAddress);
    }
}