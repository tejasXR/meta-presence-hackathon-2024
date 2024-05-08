using UnityEngine;

/// <summary>
/// Calls the NPC when the state of our hand poses meeting the 'Active' state
/// </summary>
public class NpcCaller : MonoBehaviour
{
    [SerializeField] private NpcController npcController;
    [SerializeField] private HandPoseActivator leftHandPoseActivator;
    [SerializeField] private HandPoseActivator rightHandPoseActivator;

    public enum PoseOrientation
    {
        LeftHand,
        RightHand
    }
    
    private HandPoseActivator _currentCallingPose = null;
    
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

        CancelNpcMovementToPlayer();
    }

    private void CancelNpcMovementToPlayer(bool hideDialogueOptions = true)
    {
        npcController.CancelMovement(hideDialogueOptions);
        _currentCallingPose = null;
    }
}