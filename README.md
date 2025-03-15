# GenericSoundSystem_ForUnity　🎵

## プロジェクト概要
**GenericSoundSystem_ForUnity** はUnityで使用できる汎用的なサウンドシステムです。

システムの詳細や工夫点は以下からもご確認いただけます。
[🔗 GenericSoundSystem_ForUnity 詳細（Notionページ）](https://picturesque-kayak-ac4.notion.site/195281634a16801e831bcebebff41161?pvs=4)

## 使用技術
- UniTask
- Addressable
- C#

## リポジトリ構成(フォルダのみ記述)
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
- `Test.cs`
  - ここに解説
  - ここに解説

- `Test.cs`
  - ここに解説
  - ここに解説

## 必要なライブラリについて
本リポジトリのスクリプトでは、以下のアセットを利用しているものが含まれています。
- **Test**：ここに用途を記述
