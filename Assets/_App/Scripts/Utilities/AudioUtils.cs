using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public static class AudioUtils
{
    public static void PlayRandomOneShotClipAtSource(AudioClip[] clips, AudioSource source, float volume)
    {
        var randomClip = clips[Random.Range(0, clips.Length)];
        source.PlayOneShot(randomClip, volume);
    }
    
    public static IEnumerator FadeToVolume(AudioSource source, float destinationVolume, float timeForFadeInSeconds, bool stopAudioWhenVolumeIsZero = false)
    {
        if (source.volume < destinationVolume)
        {
            while (source.volume < destinationVolume)
            {
                source.volume += Time.deltaTime / timeForFadeInSeconds;
                yield return null;
            }
        
            source.volume = destinationVolume;
        }
        else
        {
            if (source.volume > destinationVolume)
            {
                while (source.volume > destinationVolume)
                {
                    source.volume -= Time.deltaTime / timeForFadeInSeconds;
                    yield return null;
                }
        
                if (stopAudioWhenVolumeIsZero)
                    source.Stop();
                
                source.volume = destinationVolume;
            }
        }
    }
}
