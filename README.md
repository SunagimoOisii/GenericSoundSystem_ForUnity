# GenericSoundSystem_ForUnity　🎵

## 目次
- [概要](#概要)
- [使用技術](#使用技術)
- [システム構成](#システム構成)
- [プログラム　ピックアップ](#プログラム--ピックアップ)
- [セットアップ](#セットアップ)
- [基本的な使い方](#基本的な使い方)

## 概要
Unity向けサウンドシステムです。<br><br>
システムの詳細や工夫点は以下からもご確認いただけます。<br>
[🔗 GenericSoundSystem_ForUnity 詳細(Notionページ)](https://picturesque-kayak-ac4.notion.site/195281634a16801e831bcebebff41161?pvs=4)

## 使用技術
- UniTask
- Addressable
- C#

## 工夫点(上記Notionページからの抜粋)
- インターフェース(`ISoundLoader`,`ISoundCache`)による依存関係の緩和
- 音声チャンネル(`AudioSource`)をオブジェクトプールで管理(`SEManager`)

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

## プログラム--ピックアップ
- **`SoundSystem.cs`**<br>
  - **エントリーポイントとして全機能を統括**
  - `AudioMixer`のパラメータを取得,設定する機能を提供

- **`BGMManager.cs`**<br>
  - **BGM管理,フェード・クロスフェード機能を提供**

- **`SEManager.cs`**<br>
  - **SE管理,オーディオプール機能を提供**
  - **オーディオプール機能** により、不要な `AudioSource` 作成を防ぐ
 
- **`ListenerEffector.cs`**<br>
  - `ApplyFilter<T>()`により **動的に任意のオーディオフィルタを追加**
 
- **`SoundLoader.cs`**<br>
  - UniTask,Addressables を活用し、**非同期ロード / アンロードを実装**
  - `ISoundCache`との連携で、不要なロードを削減

- **`SoundCache.cs`**<br>
  - **`AudioClip`のキャッシュを管理**
  - 最終アクセス時刻を記録し、一定時間未使用のリソースを自動解放

## セットアップ<br>
### ０：前提<br>
プロジェクトにUniTask,Addressableを導入していることが前提
### １：ファイルをインポート<br>
このリポジトリの`SoundSystem.unitypackage`をUnityエディタでインポート
### ２：SoundSystem初期化<br>
`SoundSystem`を**直接インスタンス化し、** システムのセットアップ完了

## 基本的な使い方<br>
以下で言及する「アドレス」はAddressableのもの
### BGM
``` C#
//再生　　　　　　　　引数：アドレス,音量
soundSystem.BGM.Play("address", 1.0f).Forget();
//再生(フェードイン)　引数：アドレス,フェード時間,最終的な音量,
soundSystem.BGM.FadeIn("address", 2.0f, 1.0f).Forget();
//クロスフェード　　　引数：アドレス,フェード時間
soundSystem.BGM.CrossFade("address", 2.0f).Forget();
//停止
soundSystem.BGM.Stop();
```

### SE
``` C#
//再生　引数：アドレス,再生座標,音量,音程,サラウンド度合い
soundSystem.SE.Play("address", Vector3.zero, 1.0f, 1.0f, 1.0f).Forget();
```

### AudioMixer
``` C#
//パラメータ取得　引数：パラメータ名
float? volume = soundSystem.GetMixerParameter("MasterVolume");
//パラメータ変更　引数：パラメータ名,設定値
soundSystem.SetMixerParameter("MasterVolume", -10.0f);
```

### Effector
``` C#
//エフェクト適用(リバーブ)　引数：指定フィルターの設定を行う式
soundSystem.Effector.ApplyFilter<AudioReverbFilter>(filter => filter.reverbLevel = 1000f);
//エフェクト無効化
soundSystem.Effector.DisableAllEffects();
```
