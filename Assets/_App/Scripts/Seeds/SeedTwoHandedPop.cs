using System.Collections;
using Oculus.Interaction;
using UnityEngine;

public class SeedTwoHandedPop : MonoBehaviour
{
    [SerializeField] private SeedController seedController;
    [SerializeField] private Grabbable seedGrabbable;
    [SerializeField] private GameObject interiorBubble;
    [SerializeField] private float interiorSeedPullAmplitude;

    private IEnumerator _twoHandedPopRoutine;

    private void Awake()
    {
        seedGrabbable.WhenPointerEventRaised += OnGrabbablePointerEventRaised;
    }

    private void OnDestroy()
    {
        seedGrabbable.WhenPointerEventRaised -= OnGrabbablePointerEventRaised;
    }

    private void OnGrabbablePointerEventRaised(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Select)
            OnSeedGrabbed(pointerEvent.Pose);
        else if (pointerEvent.Type == PointerEventType.Unselect)
            OnSeedUngrabbed();
    }

    private void OnSeedGrabbed(Pose grabPose)
    {
        // Detect Hand Movements After Two-Handed Grab
        if (seedGrabbable.GrabPoints.Count == 2)
        {
            var realtimeGrabPose = seedGrabbable.GrabPoints;
            var initialDistanceBetweenHands = GetDistanceBetweenPoses(realtimeGrabPose[0], realtimeGrabPose[1]);
            _twoHandedPopRoutine = TwoHandedPopRoutine(initialDistanceBetweenHands);
            StartCoroutine(_twoHandedPopRoutine);
        }
        else
        {
            if (_twoHandedPopRoutine != null)
            {
                StopCoroutine(_twoHandedPopRoutine);
            }
        }
    }

    private IEnumerator TwoHandedPopRoutine(float initialDistanceBetweenHands)
    {
        while (seedGrabbable.GrabPoints.Count == 2)
        {
            var realtimeGrabPose = seedGrabbable.GrabPoints;
            var currentDistanceBetweenHands = GetDistanceBetweenPoses(realtimeGrabPose[0], realtimeGrabPose[1]);
            var distanceDelta = currentDistanceBetweenHands - initialDistanceBetweenHands;
            
            if (distanceDelta < 0)
                distanceDelta = 0;
            
            interiorBubble.transform.localScale = Vector3.one * (.001F + distanceDelta) * interiorSeedPullAmplitude;
            
            if (Vector3.Distance(interiorBubble.transform.lossyScale, transform.lossyScale) < .001F 
                || interiorBubble.transform.lossyScale.magnitude >= transform.lossyScale.magnitude)
            {
                // Simulate flinging the seed towards the ceiling...
                // TEJAS: TODO Need to refactor method names!!
                seedController.FlungTowardsCeiling();
                yield break;
            }
            
            yield return new WaitForEndOfFrame();
        }
    }

    private void OnSeedUngrabbed()
    {
        interiorBubble.transform.localScale = Vector3.zero;
    }

    private float GetDistanceBetweenPoses(Pose firstPose, Pose secondPose)
    {
        return Vector3.Distance(firstPose.position, secondPose.position);
    }
}