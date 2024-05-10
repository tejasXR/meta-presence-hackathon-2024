using UnityEngine;

public class NpcOptionMenuUIAudio : MonoBehaviour
{
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioData uiSelect;
    [SerializeField] private AudioData uiUnselect;

    public void OnWhenSelect()
    {
        AudioUtils.PlayRandomOneShotClipAtSource(uiSelect.Clips, uiAudioSource, uiSelect.Volume);
    }

    public void OnWhenUnselect()
    {
        AudioUtils.PlayRandomOneShotClipAtSource(uiUnselect.Clips, uiAudioSource, uiUnselect.Volume);
    }
}
