using UnityEngine;
using UnityEngine.Audio;
using Cysharp.Threading.Tasks;

public class Test_SoundSystem : MonoBehaviour
{
    [SerializeField] private AudioMixer testMixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup seGroup;
    [SerializeField] private string testBGMAddress = "TestBGM";
    [SerializeField] private string testBGMAddressFadeIn = "TestBGM_FadeIn";
    [SerializeField] private string testSEAddress = "TestSE";

    private SoundSystem soundSystem;

    private async void Start()
    {
        Debug.Log("SoundSystem�e�X�g�J�n");

        //�L���b�V���ƃ��[�_�[�̏���
        var soundCache  = new SoundCache();
        var soundLoader = new SoundLoader(soundCache);

        //SoundSystem������
        soundSystem = new(
            soundCache,
            soundLoader,
            testMixer,
            bgmGroup,
            seGroup,
            5
        );

        //�e�@�\�e�X�g
        await TestBGMPlayback();
        await TestSEPlayback();
        TestListenerEffector();

        Debug.Log("SoundSystem�e�X�g����");
    }

    private async UniTask TestBGMPlayback()
    {
        Debug.Log("BGM�Đ��e�X�g�J�n");

        await soundSystem.BGM.Play(testBGMAddress, 0.8f);
        Debug.Log("BGM�Đ����s");

        await UniTask.Delay(2000);
        await soundSystem.BGM.CrossFade(testBGMAddressFadeIn, 3.0f);
        Debug.Log("BGM�N���X�t�F�[�h���s");

        await UniTask.Delay(2000);
        soundSystem.BGM.Stop();
        Debug.Log("BGM��~���s");

        Debug.Log("BGM�Đ��e�X�g����");
    }

    private async UniTask TestSEPlayback()
    {
        Debug.Log("SE�Đ��e�X�g�J�n");

        for (int i = 0; i < 5; i++)
        {
            await soundSystem.SE.Play(testSEAddress, Vector3.zero, 0.5f + 0.1f * i, 1.0f, 1.0f);
            Debug.Log($"SE {i + 1} �Đ����s");
            await UniTask.Delay(500);
        }

        Debug.Log("SE�Đ��e�X�g����");
    }

    private void TestListenerEffector()
    {
        Debug.Log("ListenerEffector�e�X�g�J�n");

        soundSystem.Effector.ApplyFilter<AudioEchoFilter>(filter =>
        {
            filter.delay = 500.0f;
            filter.decayRatio = 0.5f;
        });
        Debug.Log("AudioListener��AudioEchoFilter�K�p");
        Debug.Log("ListenerEffector�e�X�g����");

        soundSystem.SetMixerParameter("MasterVolume", -10.0f);
        Debug.Log("MasterVolume�p�����[�^�ύX���s");

        var masterVolume = soundSystem.GetMixerParameter("MasterVolume");
        Debug.Log($"�p�����[�^�擾 MasterVolume: {masterVolume}");

        soundSystem.Effector.DisableAllEffects();
        Debug.Log("�S�G�t�F�N�g���������s");
    }
}