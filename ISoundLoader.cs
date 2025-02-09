using Cysharp.Threading.Tasks;
using UnityEngine;

public interface ISoundLoader
{
    UniTask<AudioClip> LoadClip(string resourceAddress);

    void UnloadClip(string resourceAddress);
}