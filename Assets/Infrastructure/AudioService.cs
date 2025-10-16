using UnityEngine;

public class AudioService : IAudioService
{
    public void Play(string key, int priority = 1, bool loop = false)
    {
        Debug.Log($"[AudioService] Play '{key}' (prio {priority}, loop {loop})");
    }

    public void Stop(string key)
    {
        Debug.Log($"[AudioService] Stop '{key}'");
    }
}
