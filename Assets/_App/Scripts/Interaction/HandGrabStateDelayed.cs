using System.Collections;
using Oculus.Interaction;
using UnityEngine;

public class HandGrabStateDelayed : MonoBehaviour, IActiveState
{
    public bool Active { get; private set; }

    [SerializeField] private HandGrabState handGrabState;

    private IEnumerator _delayedStateRoutine;

    private void Awake()
    {
        handGrabState.GrabbingActive += OnGrabbingActivated;
        handGrabState.GrabbingDeactived += OnGrabbingDeactivated;
    }

    private void OnGrabbingActivated()
    {
        if (_delayedStateRoutine != null)
            StopCoroutine(_delayedStateRoutine);

        _delayedStateRoutine = DelayedStateChanged(true);
        StartCoroutine(_delayedStateRoutine);
    }

    private void OnGrabbingDeactivated()
    {
        if (_delayedStateRoutine != null)
            StopCoroutine(_delayedStateRoutine);

        _delayedStateRoutine = DelayedStateChanged(false);
        StartCoroutine(_delayedStateRoutine);
    }

    private IEnumerator DelayedStateChanged(bool state)
    {
        yield return new WaitForSeconds(.5F);
        Active = state;
    }
}
