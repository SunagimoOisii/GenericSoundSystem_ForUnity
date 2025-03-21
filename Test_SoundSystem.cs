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
        Debug.Log("SoundSystemテスト開始");

        //キャッシュとローダーの準備
        var soundCache  = new SoundCache();
        var soundLoader = new SoundLoader(soundCache);

        //SoundSystem初期化
        soundSystem = new(
            soundCache,
            soundLoader,
            testMixer,
            bgmGroup,
            seGroup,
            5
        );

        //各機能テスト
        await TestBGMPlayback();
        await TestSEPlayback();
        TestListenerEffector();

        Debug.Log("SoundSystemテスト完了");
    }

    private async UniTask TestBGMPlayback()
    {
        Debug.Log("BGM再生テスト開始");

        await soundSystem.BGM.Play(testBGMAddress, 0.8f);
        Debug.Log("BGM再生実行");

        await UniTask.Delay(2000);
        await soundSystem.BGM.CrossFade(testBGMAddressFadeIn, 3.0f);
        Debug.Log("BGMクロスフェード実行");

        await UniTask.Delay(2000);
        soundSystem.BGM.Stop();
        Debug.Log("BGM停止実行");

        Debug.Log("BGM再生テスト完了");
    }

    private async UniTask TestSEPlayback()
    {
        Debug.Log("SE再生テスト開始");

        for (int i = 0; i < 5; i++)
        {
            await soundSystem.SE.Play(testSEAddress, Vector3.zero, 0.5f + 0.1f * i, 1.0f, 1.0f);
            Debug.Log($"SE {i + 1} 再生実行");
            await UniTask.Delay(500);
        }

        Debug.Log("SE再生テスト完了");
    }

    private void TestListenerEffector()
    {
        Debug.Log("ListenerEffectorテスト開始");

        soundSystem.Effector.ApplyFilter<AudioEchoFilter>(filter =>
        {
            filter.delay = 500.0f;
            filter.decayRatio = 0.5f;
        });
        Debug.Log("AudioListenerにAudioEchoFilter適用");
        Debug.Log("ListenerEffectorテスト完了");

        soundSystem.SetMixerParameter("MasterVolume", -10.0f);
        Debug.Log("MasterVolumeパラメータ変更実行");

        var masterVolume = soundSystem.GetMixerParameter("MasterVolume");
        Debug.Log($"パラメータ取得 MasterVolume: {masterVolume}");

        soundSystem.Effector.DisableAllEffects();
        Debug.Log("全エフェクト無効化実行");
    }
}