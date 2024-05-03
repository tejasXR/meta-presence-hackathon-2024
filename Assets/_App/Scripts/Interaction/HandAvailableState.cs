using System;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class HandAvailableState : MonoBehaviour, IActiveState
{
    [SerializeField] private TouchHandGrabInteractor touchHandGrabInteractor;
    [SerializeField] private HandGrabInteractor handGrabInteractor;
    [SerializeField] private DistanceHandGrabInteractor distanceHandGrabInteractor;

    public bool Active => !_isGrabbing;

    private bool _isGrabbing;
    
    private void Awake()
    {
        touchHandGrabInteractor.WhenStateChanged += OnInteractorStateChanged;
        handGrabInteractor.WhenStateChanged += OnInteractorStateChanged;
        distanceHandGrabInteractor.WhenStateChanged += OnInteractorStateChanged;
    }

    private void OnDestroy()
    {
        if (touchHandGrabInteractor)
            touchHandGrabInteractor.WhenStateChanged -= OnInteractorStateChanged;
        
        if (handGrabInteractor)
            handGrabInteractor.WhenStateChanged -= OnInteractorStateChanged;
        
        if (distanceHandGrabInteractor)
            distanceHandGrabInteractor.WhenStateChanged -= OnInteractorStateChanged;
    }

    private void OnInteractorStateChanged(InteractorStateChangeArgs interactorStateChangeArgs)
    {
        switch (interactorStateChangeArgs.NewState)
        {
            case InteractorState.Normal:
                UnregisterGrab();
                break;
            case InteractorState.Hover:
                break;
            case InteractorState.Select:
                RegisterGrab();
                break;
            case InteractorState.Disabled:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void RegisterGrab()
    {
        _isGrabbing = true;
    }

    private void UnregisterGrab()
    {
        if (touchHandGrabInteractor.HasInteractable | distanceHandGrabInteractor.HasInteractable | handGrabInteractor.HasInteractable)
            return;
        
        _isGrabbing = false;
    }
}
