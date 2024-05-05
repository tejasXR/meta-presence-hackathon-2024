using System;
using Oculus.Interaction;
using UnityEngine;

public class HandPoseActivator: MonoBehaviour, IActiveState
{
    public event Action<HandPoseActivator, Transform> PoseActivated;
    public event Action<HandPoseActivator> PoseDeactivated;

    [SerializeField] private ActiveStateSelector[] poseStateSelectors;
    [SerializeField] private Transform posePoint;
    
    public bool Active { get; private set; }

    private int _activePoses;
    
    private void Awake()
    {
        foreach (var activeStateSelector in poseStateSelectors)
        {
            activeStateSelector.WhenSelected += ActivatePose;
            activeStateSelector.WhenUnselected += DeactivatePose;
        }
    }

    private void OnDestroy()
    {
        foreach (var activeStateSelector in poseStateSelectors)
        {
            activeStateSelector.WhenSelected -= ActivatePose;
            activeStateSelector.WhenUnselected -= DeactivatePose;
        }
    }

    private void ActivatePose()
    {
        _activePoses++;

        if (_activePoses == poseStateSelectors.Length)
        {
            Active = true;
            PoseActivated?.Invoke(this, posePoint);
        }
    }

    private void DeactivatePose()
    {
        _activePoses--;
        
        Active = false;
        PoseDeactivated?.Invoke(this);
    }

}