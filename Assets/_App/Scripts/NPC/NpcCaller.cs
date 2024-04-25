using UnityEngine;

/// <summary>
/// Calls the NPC when the state of our hand poses meeting the 'Active' state
/// </summary>
public class NpcCaller : MonoBehaviour
{
    [SerializeField] private NpcController npcController;
    [SerializeField] private HandPosePoint leftHandPosePoint;
    [SerializeField] private HandPosePoint rightHandPosePoint;

    private HandPosePoint _currentCallingPose;
    
    private void Awake()
    {
        leftHandPosePoint.PoseActivated += OnPoseActivated;
        leftHandPosePoint.PoseDeactivated += OnPoseDeactivated;
        
        rightHandPosePoint.PoseActivated += OnPoseActivated;
        rightHandPosePoint.PoseDeactivated += OnPoseDeactivated;
    }

    private void OnDestroy()
    {
        if (leftHandPosePoint)
        {
            leftHandPosePoint.PoseActivated -= OnPoseActivated;
            leftHandPosePoint.PoseDeactivated -= OnPoseDeactivated;
        }

        if (rightHandPosePoint)
        {
            rightHandPosePoint.PoseActivated -= OnPoseActivated;
            rightHandPosePoint.PoseDeactivated -= OnPoseDeactivated;
        }
    }

    private void OnPoseActivated(HandPosePoint handPosePoint, Transform callPoint)
    {
        if (_currentCallingPose == handPosePoint)
        {
            return;
        }

        CancelNpcMovementToPlayer();
        npcController.MoveToPoint(callPoint, NpcController.MovementTypeEnum.MovingToPlayer);
    }

    private void OnPoseDeactivated(HandPosePoint handPosePoint)
    {
        if (_currentCallingPose != handPosePoint)
            return;

        CancelNpcMovementToPlayer();
    }

    private void CancelNpcMovementToPlayer(bool hideDialogueOptions = true)
    {
        npcController.CancelMovement(hideDialogueOptions);
    }
}