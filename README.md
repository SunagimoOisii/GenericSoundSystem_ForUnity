# GenericSoundSystem_ForUnity　🎵

## プロジェクト概要
**GenericSoundSystem_ForUnity** はUnityで使用できる汎用的なサウンドシステムです。

システムの詳細や工夫点は以下からもご確認いただけます。<br>
[🔗 GenericSoundSystem_ForUnity 詳細（Notionページ）](https://picturesque-kayak-ac4.notion.site/195281634a16801e831bcebebff41161?pvs=4)

## 使用技術
- UniTask
- Addressable
- C#

## システム構成
``` mermaid
classDiagram
    %% SoundSystemを頂点として各クラスを描画
    class SoundSystem {
        +BGMManager BGM
        +SEManager SE
        +ListenerEffector Effector
        +ISoundLoader Loader
        +ISoundCache Cache
    }

    %% BGMManagerとその関係
    class BGMManager {
        -ISoundLoader loader
        +Play(string, float)
        +Stop()
        +CrossFade(string, float)
    }

    %% SEManagerとその関係
    class SEManager {
        -ISoundLoader loader
        -Queue~AudioSource~ audioSourcePool
        +Play(string, Vector3, float, float, float)
    }

    %% ListenerEffectorとその関係
    class ListenerEffector {
        +ApplyFilter~T~(Action~T~)
        +DisableAllEffects()
        +SetMixerParameter(string, float)
        +GetMixerParameter(string): float?
    }

    %% SoundLoaderとその関係
    class SoundLoader {
        -ISoundCache cache
        +LoadClip(string): UniTask~AudioClip~
        +UnloadClip(string)
    }

    %% SoundCacheとその関係
    class SoundCache {
        +Retrieve(string): AudioClip
        +Add(string, AudioClip)
        +Remove(string)
        +CleanupUnused(float)
        +Clear()
    }

    %% インターフェースの定義
    class ISoundLoader {
        <<interface>>
        +LoadClip(string): UniTask~AudioClip~
        +UnloadClip(string)
    }

    class ISoundCache {
        <<interface>>
        +Retrieve(string): AudioClip
        +Add(string, AudioClip)
        +Remove(string)
        +CleanupUnused(float)
        +Clear()
    }

    %% クラス間の関係を定義
    SoundSystem --> BGMManager : 管理
    SoundSystem --> SEManager : 管理
    SoundSystem --> ListenerEffector : 管理
    SoundSystem --> ISoundLoader : 使用
    SoundSystem --> ISoundCache : 使用

    BGMManager --> ISoundLoader : 依存
    SEManager --> ISoundLoader : 依存

    SoundLoader --> ISoundCache : 依存

    SoundLoader ..|> ISoundLoader : 実装
    SoundCache ..|> ISoundCache : 実装

```

## プログラム　ピックアップ
- `SoundSystem.cs`<br>
**エントリーポイントとして全サウンド管理を統括するクラス**
  - **BGM / SE / AudioListenerEffector の統括管理**
  - `AudioMixer` のパラメータを取得・設定するメソッドを提供

- `BGMManager.cs`<br>
**BGM の管理、フェード / クロスフェード機能を提供**
  - `UniTask` を活用した **非同期フェード処理**
  - **BGM のクロスフェード機能** を搭載

- `SEManager.cs`<br>
**効果音の管理とプール制御**
  - **オーディオプール機能** により、不要な `AudioSource` 作成を防ぐ
  - **一定時間未使用の AudioSource を削除する自動クリーンアップ**
 
- `ListenerEffector.cs`<br>
**リアルタイムで `AudioListener` にエフェクトを適用**
  - `ApplyFilter<T>()` により **任意のオーディオフィルタを追加**
  - `DisableAllEffects()` でフィルターを一括無効化可能
 
- `SoundLoader.cs`<br>
**非同期リソースロードを担当**
  - Addressables を活用し、**非同期ロード / アンロードを実装**
  - `ISoundCache` との連携により、不要なロードを削減

- `SoundCache.cs`<br>
**ロード済み `AudioClip` のキャッシュを管理**
  - 最終アクセス時刻を記録し、一定時間未使用のリソースを自動解放
  - Addressables に対応した **リソース管理**
  - 不要な `AudioClip` をメモリから開放し、メモリリークを防止
