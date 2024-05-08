using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// Flips Active to TRUE when we are not grabbing anything
/// </summary>
public class HandGrabStateDeactivated : MonoBehaviour, IActiveState
{
    public bool Active { get; private set; }

    [SerializeField] private HandGrabState handGrabState;

    private void Awake()
    {
        handGrabState.GrabbingActive += OnGrabbingActivated;
        handGrabState.GrabbingDeactived += OnGrabbingDeactivated;
    }

    private void OnGrabbingActivated()
    {
        Active = false;
    }

    private void OnGrabbingDeactivated()
    {
        Active = true;
    }
}
