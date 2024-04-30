using System;
using Oculus.Interaction;
using UnityEngine;

public class HandPoseActivator: MonoBehaviour
{
    public event Action<HandPoseActivator, Transform> PoseActivated;
    public event Action<HandPoseActivator> PoseDeactivated;

    [SerializeField] private ActiveStateSelector poseStateSelector;
    [SerializeField] private Transform posePoint;
    
    public bool PoseActive { get; private set; }
    
    private void Awake()
    {
        poseStateSelector.WhenSelected += ActivatePose;
        poseStateSelector.WhenUnselected += DeactivatePose;
    }

    private void OnDestroy()
    {
        if (poseStateSelector)
        {
            poseStateSelector.WhenSelected -= ActivatePose;
            poseStateSelector.WhenUnselected -= DeactivatePose;
        }
    }

    private void ActivatePose()
    {
        PoseActive = true;
        PoseActivated?.Invoke(this, posePoint);
    }

    private void DeactivatePose()
    {
        PoseActive = false;
        PoseDeactivated?.Invoke(this);
    }
}