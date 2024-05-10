using System;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class SeedAudio : MonoBehaviour
{
    [SerializeField] private AudioSource seedAudioSource;
    [SerializeField] private AudioData seedAppear;
    [SerializeField] private AudioData grabSeedAudio;
    [SerializeField] private AudioData hoverSeedAudio;
    [SerializeField] private AudioData seedFlungAudio;
    [SerializeField] private AudioData seedPoppingOnCeilingAudio;
    [SerializeField] private AudioData seedLifecycleCompletedAudio;
    [SerializeField] private AudioData absorbedSeedAudio;
    [Space]
    [SerializeField] private Grabbable grabbable;
    [SerializeField] private SeedController seedController;
    [SerializeField] private SeedMorph seedMorph;

    private void Awake()
    {
        // The mix-n-match use of UnityEvents and System.Action events are getting messy
        // TODO: Tejas - cleanup event declaration and usage
        grabbable.WhenPointerEventRaised += OnGrabbablePointerEventRaised;
        seedController.OnSeedFlung.AddListener(OnSeedFlug);
        seedController.OnSeedPoppedOnTheCeiling.AddListener(OnSeedPoppedOnCeiling);
        seedController.OnSeedLifecycleCompleted.AddListener(OnSeedLifecycleCompleted);
        seedMorph.AbsorbedSeed += OnAbsorbedSeed;
    }

    private void OnDestroy()
    {
        if (grabbable)
            grabbable.WhenPointerEventRaised -= OnGrabbablePointerEventRaised;

        if (seedController)
        {
            seedController.OnSeedFlung.RemoveListener(OnSeedFlug);
            seedController.OnSeedPoppedOnTheCeiling.RemoveListener(OnSeedPoppedOnCeiling);
            seedController.OnSeedLifecycleCompleted.RemoveListener(OnSeedLifecycleCompleted);
        }
        
        if (seedMorph)
            seedMorph.AbsorbedSeed -= OnAbsorbedSeed;
    }

    private void OnEnable()
    {
        AudioUtils.PlayRandomOneShotClipAtSource(seedAppear.Clips, seedAudioSource, seedAppear.Volume);
    }

    private void OnGrabbablePointerEventRaised(PointerEvent pointerEvent)
    {
        switch (pointerEvent.Type)
        {
            case PointerEventType.Hover:
                var interactorType = pointerEvent.Data.GetType();
                if (interactorType == typeof(DistanceHandGrabInteractor))
                    AudioUtils.PlayRandomOneShotClipAtSource(hoverSeedAudio.Clips, seedAudioSource, hoverSeedAudio.Volume);
                break;
            case PointerEventType.Unhover:
                break;
            case PointerEventType.Select:
                AudioUtils.PlayRandomOneShotClipAtSource(grabSeedAudio.Clips, seedAudioSource, grabSeedAudio.Volume);
                break;
            case PointerEventType.Unselect:
                break;
            case PointerEventType.Move:
                break;
            case PointerEventType.Cancel:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnSeedFlug(SeedController _)
    {
        AudioUtils.PlayRandomOneShotClipAtSource(seedFlungAudio.Clips, seedAudioSource, grabSeedAudio.Volume);
    }
    
    private void OnSeedPoppedOnCeiling(SeedController _, Vector3 __)
    {
        AudioUtils.PlayRandomOneShotClipAtSource(seedPoppingOnCeilingAudio.Clips, seedAudioSource, seedPoppingOnCeilingAudio.Volume);
    }

    private void OnSeedLifecycleCompleted(SeedController arg0)
    {
        AudioUtils.PlayRandomOneShotClipAtSource(seedLifecycleCompletedAudio.Clips, seedAudioSource, seedLifecycleCompletedAudio.Volume);
    }

    private void OnAbsorbedSeed()
    {
        AudioUtils.PlayRandomOneShotClipAtSource(absorbedSeedAudio.Clips, seedAudioSource, seedLifecycleCompletedAudio.Volume);
    }
}
