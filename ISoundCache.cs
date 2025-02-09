using UnityEngine;

public interface ISoundCache
{
    AudioClip Retrieve(string resourceAddress);

    void Add(string resourceAddress, AudioClip clip);

    void Remove(string resourceAddress);

    void CleanupUnused(float idleTimeThreshold);

    void Clear();
}