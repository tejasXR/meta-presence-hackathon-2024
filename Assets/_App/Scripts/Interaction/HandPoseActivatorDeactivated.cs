using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// Flips Active to TRUE when the associated pose activator is deactivated
/// </summary>
public class HandPoseActivatorDeactivated : MonoBehaviour, IActiveState
{
    public bool Active { get; private set; }

    [SerializeField] private HandPoseActivator handPoseActivator;

    private void Awake()
    {
        handPoseActivator.PoseActivated += OnPoseActivated;
        handPoseActivator.PoseDeactivated += OnPoseDeactivated;
    }

    private void OnPoseActivated(HandPoseActivator arg1, Transform arg2)
    {
        Active = false;
    }

    private void OnPoseDeactivated(HandPoseActivator poseActivator)
    {
        Active = true;
    }
}
