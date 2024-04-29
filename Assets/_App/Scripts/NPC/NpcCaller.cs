using UnityEngine;

/// <summary>
/// Calls the NPC when the state of our hand poses meeting the 'Active' state
/// </summary>
public class NpcCaller : MonoBehaviour
{
    [SerializeField] private NpcController npcController;
    [SerializeField] private HandPosePoint leftHandPosePoint;
    [SerializeField] private HandPosePoint rightHandPosePoint;

    public enum PoseOrientation
    {
        LeftHand,
        RightHand
    }
    
    private HandPosePoint _currentCallingPose = null;
    
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
        if (_currentCallingPose != null && _currentCallingPose == handPosePoint)
        {
            return;
        }

        CancelNpcMovementToPlayer();

        PoseOrientation orientation = handPosePoint == leftHandPosePoint ? PoseOrientation.LeftHand : PoseOrientation.RightHand;
        npcController.SetDialogueOrientation(orientation);
        npcController.MoveToPoint(callPoint, NpcController.MovementTypeEnum.MovingToPlayer);

        _currentCallingPose = handPosePoint;
    }

    private void OnPoseDeactivated(HandPosePoint handPosePoint)
    {
        if (_currentCallingPose != null && _currentCallingPose != handPosePoint)
            return;

        CancelNpcMovementToPlayer();
    }

    private void CancelNpcMovementToPlayer(bool hideDialogueOptions = true)
    {
        npcController.CancelMovement(hideDialogueOptions);
        _currentCallingPose = null;
    }
}