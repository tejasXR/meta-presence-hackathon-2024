using System.Collections;
using UnityEngine;

/// <summary>
/// Calls the NPC when the state of our hand poses meeting the 'Active' state
/// </summary>
public class NpcCaller : MonoBehaviour
{
    [SerializeField] private NpcController npcController;
    [SerializeField] private HandPoseActivator leftHandPoseActivator;
    [SerializeField] private HandPoseActivator rightHandPoseActivator;
    
    private const float REQUIRED_ACTIVE_STATE_TIME = .25F;

    private IEnumerator _checkActiveTimeRoutine;
    private HandPoseActivator _currentCallingPose = null;
    private float _currentActiveTime;
    
    public enum PoseOrientation
    {
        LeftHand,
        RightHand
    }

    private void Awake()
    {
        leftHandPoseActivator.PoseActivated += OnPoseActivated;
        leftHandPoseActivator.PoseDeactivated += OnPoseDeactivated;
        
        rightHandPoseActivator.PoseActivated += OnPoseActivated;
        rightHandPoseActivator.PoseDeactivated += OnPoseDeactivated;
    }

    private void OnDestroy()
    {
        if (leftHandPoseActivator)
        {
            leftHandPoseActivator.PoseActivated -= OnPoseActivated;
            leftHandPoseActivator.PoseDeactivated -= OnPoseDeactivated;
        }

        if (rightHandPoseActivator)
        {
            rightHandPoseActivator.PoseActivated -= OnPoseActivated;
            rightHandPoseActivator.PoseDeactivated -= OnPoseDeactivated;
        }
    }

    private void OnPoseActivated(HandPoseActivator handPoseActivator, Transform callPoint)
    {
        if (_currentCallingPose != null && _currentCallingPose == handPoseActivator)
        {
            return;
        }

        // TEJAS: messy call, but works

        if (_checkActiveTimeRoutine != null)
            StopCoroutine(_checkActiveTimeRoutine);

        _checkActiveTimeRoutine = CheckActiveTime(handPoseActivator, callPoint);
        StartCoroutine(_checkActiveTimeRoutine);
    }

    private IEnumerator CheckActiveTime(HandPoseActivator handPoseActivator, Transform callPoint)
    {
        while (_currentActiveTime < REQUIRED_ACTIVE_STATE_TIME)
        {
            _currentActiveTime += Time.deltaTime;
            yield return null;
        }
        
        CallNpc(handPoseActivator, callPoint);
    }

    private void CallNpc(HandPoseActivator handPoseActivator, Transform callPoint)
    {
        CancelNpcMovementToPlayer();

        PoseOrientation orientation = handPoseActivator == leftHandPoseActivator ? PoseOrientation.LeftHand : PoseOrientation.RightHand;
        npcController.SetNpcMenuOrientation(orientation);
        npcController.MoveToPoint(callPoint, NpcController.MovementTypeEnum.MovingToPlayer);

        _currentCallingPose = handPoseActivator;
    }

    private void OnPoseDeactivated(HandPoseActivator handPoseActivator)
    {
        if (_currentCallingPose != null && _currentCallingPose != handPoseActivator)
            return;

        if (_checkActiveTimeRoutine != null)
        {
            StopCoroutine(_checkActiveTimeRoutine);
            _checkActiveTimeRoutine = null;
        }
        
        _currentActiveTime = 0;

        CancelNpcMovementToPlayer();
    }

    private void CancelNpcMovementToPlayer(bool hideDialogueOptions = true)
    {
        npcController.CancelMovement(hideDialogueOptions);
        _currentCallingPose = null;
    }
}