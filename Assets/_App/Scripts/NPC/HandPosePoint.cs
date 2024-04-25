using System;
using Oculus.Interaction;
using UnityEngine;

public class HandPosePoint: MonoBehaviour
{
    public event Action<HandPosePoint, Transform> PoseActivated;
    public event Action<HandPosePoint> PoseDeactivated;

    [SerializeField] private ActiveStateSelector poseStateSelector;
    [SerializeField] private Transform posePoint;
    
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
        PoseActivated?.Invoke(this, posePoint);
    }

    private void DeactivatePose()
    {
        PoseDeactivated?.Invoke(this);
    }
}