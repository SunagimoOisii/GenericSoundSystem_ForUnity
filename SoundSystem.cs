using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// �T�E���h�Ǘ��̃G���g���[�|�C���g��񋟂���N���X<para></para>
/// - �e�}�l�[�W���Ƌ@�\�C���^�[�t�F�[�X���O������󂯎�蓝��I�ɊǗ�<para></para>
/// - �{���W���[���ɓ�����AudioMixer�I�u�W�F�N�g�̎g�p��O��Ƃ��Ă���
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
            Debug.LogWarning($"SoundSystem: �p�����[�^ '{exposedParamName}' �̎擾�Ɏ��s");
            return null;
        }
    }

    public void SetMixerParameter(string exposedParamName, float value)
    {
        if (mixer.SetFloat(exposedParamName, value) == false)
        {
            Debug.LogWarning($"SoundSystem: �p�����[�^ '{exposedParamName}' �̐ݒ�Ɏ��s");
        }
    }
}